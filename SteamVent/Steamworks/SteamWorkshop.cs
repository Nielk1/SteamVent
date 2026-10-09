using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.Steamworks;
using SteamVent.Steamworks.Native;
using SteamVent.Steamworks.Native.Interfaces;

namespace SteamVent.Steamworks
{
    /// <summary>
    /// Thin, in-process wrapper over the Steamworks <see cref="ISteamUGC"/> interop, mirroring
    /// <see cref="SteamApps"/> for the workshop. Async Steam calls are started on <see cref="ISteamUGC"/>
    /// and awaited by polling <see cref="ISteamUtils.IsAPICallCompleted"/> before the result struct is read
    /// back through <c>Steam.GetAPICallResult</c> - the same mechanism the Bridge child process uses, but
    /// without the process hop.
    /// <para>
    /// This is only eligible when the calling process is itself running under the target app (see
    /// <see cref="SteamVentOptions.SteamworksAppId"/>); the routing layer
    /// (<see cref="SteamContext.GetWorkshopCommands(UInt32)"/>) picks this path or falls back to the Bridge.
    /// </para>
    /// </summary>
    public sealed class SteamWorkshop : IWorkshopCommands, IWorkshopSource
    {
        private static readonly int PollIntervalMs = 100;
        private static readonly int InstallPollIntervalMs = 500;
        private static readonly TimeSpan DownloadDeadline = TimeSpan.FromMinutes(10);

        private readonly SteamClient _client;

        public SteamWorkshop(SteamClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        #region IWorkshopCommands

        /// <summary>Subscribes to a workshop item. The call completes when Steam acknowledges the subscription.</summary>
        public Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
        {
            var (ugc, utils) = RequireUgcAndUtils();
            EnsureAppIdContext(appid, utils, "workshop subscribe", publishedFileId);
            UInt64 call = ugc.SubscribeItem(publishedFileId);
            return AwaitCallAsync<RemoteStorageSubscribePublishedFileResult_t>(utils, call, RemoteStorageSubscribePublishedFileResult_t.k_iCallback, cancellationToken);
        }

        /// <summary>Unsubscribes from a workshop item, removing its installed content.</summary>
        public Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
        {
            var (ugc, utils) = RequireUgcAndUtils();
            EnsureAppIdContext(appid, utils, "workshop unsubscribe", publishedFileId);
            UInt64 call = ugc.UnsubscribeItem(publishedFileId);
            return AwaitCallAsync<RemoteStorageUnsubscribePublishedFileResult_t>(utils, call, RemoteStorageUnsubscribePublishedFileResult_t.k_iCallback, cancellationToken);
        }

        /// <summary>
        /// Downloads (installs) a workshop item and completes only once it is fully installed and up to date - the
        /// same "started, then done / failed / cancelled" contract as the Bridge, so the caller cannot observe a
        /// premature "done". Reports <paramref name="progress"/> samples while the content transfers.
        /// </summary>
        public async Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, IProgress<WorkshopItemProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            var (ugc, utils) = RequireUgcAndUtils();
            EnsureAppIdContext(appid, utils, "workshop download", publishedFileId);

            if (IsInstalledAndCurrent(ugc.GetItemState(publishedFileId)))
                return new DownloadItemResult_t { m_eResult = EResult.k_EResultOK, m_nPublishedFileId = publishedFileId };

            UInt64 call = ugc.DownloadItem(publishedFileId);
            DownloadItemResult_t? maybeResult = await TryAwaitCallAsync<DownloadItemResult_t>(utils, call, DownloadItemResult_t.k_iCallback, cancellationToken);

            // The acknowledgement is the fast path. A transiently-reported failure (e.g. the item is already
            // DownloadPending because the subscription just applied) is not a dead end: if a download is actually
            // in flight we rely on the *installed* end-state below rather than the ACK, so the caller sees a clean
            // "started, then done" instead of a spurious failure.
            if (maybeResult is null || maybeResult.Value.m_eResult != EResult.k_EResultOK)
            {
                if (!IsDownloadActive(ugc.GetItemState(publishedFileId)))
                {
                    string detail = maybeResult is { } r ? $"returned {r.m_eResult}" : "failed before completing";
                    throw new WorkshopException($"DownloadItem({publishedFileId}) {detail} (item {DescribeItem(publishedFileId)}).", null)
                    {
                        AppId = appid > 0 ? (UInt32)appid : null,
                        PublishedFileId = publishedFileId,
                        Operation = "workshop download",
                    };
                }
            }

            DownloadItemResult_t result = maybeResult
                ?? new DownloadItemResult_t { m_eResult = EResult.k_EResultOK, m_nPublishedFileId = publishedFileId };

            DateTime deadline = DateTime.UtcNow + DownloadDeadline;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                EItemState state = ugc.GetItemState(publishedFileId);
                if (IsInstalledAndCurrent(state))
                    return result;

                if (!IsDownloadActive(state))
                    throw new WorkshopException($"The download for {publishedFileId} stopped before completing (item {DescribeItem(publishedFileId)}).", null)
                    {
                        AppId = appid > 0 ? (UInt32)appid : null,
                        PublishedFileId = publishedFileId,
                        Operation = "workshop download",
                    };

                UInt64 bytesDownloaded = 0, bytesTotal = 0;
                ugc.GetItemDownloadInfo(publishedFileId, ref bytesDownloaded, ref bytesTotal);
                progress?.Report(new WorkshopItemProgress
                {
                    PublishedFileId = publishedFileId,
                    State = state,
                    BytesDownloaded = bytesDownloaded,
                    BytesTotal = bytesTotal,
                });

                if (DateTime.UtcNow > deadline)
                    throw new TimeoutException($"Timed out waiting for the download of {publishedFileId} to finish.");

                await Task.Delay(InstallPollIntervalMs, cancellationToken);
            }
        }

        #endregion IWorkshopCommands

        #region IWorkshopSource

        public Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => Task.FromResult(BuildItems());

        public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => GetInstalledWorkshopItems(appid, cancellationToken);

        #endregion IWorkshopSource

        #region Helpers

        private (ISteamUGC Ugc, ISteamUtils Utils) RequireUgcAndUtils()
        {
            ISteamUGC? ugc = _client.GetISteamUGC();
            if (ugc == null)
                throw new WorkshopException("Steamworks is not available (Steam not running or the ISteamUGC interface could not be created) for in-process workshop commands.");
            ISteamUtils? utils = _client.GetISteamUtils();
            if (utils == null)
                throw new WorkshopException("Steamworks ISteamUtils is not available for in-process workshop commands.");
            return (ugc, utils);
        }

        /// <summary>
        /// Guard: when the caller asked for a specific AppID, verify the live Steam context is actually running
        /// under it - if <c>ISteamUtils.GetAppID</c> differs, a stale <c>steam_appid.txt</c> (or another game) got in
        /// the way of the forced context, and we must fail loudly rather than operate on the wrong app's workshop.
        /// When no specific AppID was requested (<paramref name="appid"/> is 0), we simply operate under whatever
        /// context Steam is running under.
        /// </summary>
        private static void EnsureAppIdContext(int appid, ISteamUtils utils, string operation, ulong? publishedFileId)
        {
            if (appid <= 0)
                return;

            UInt32 live = utils.GetAppID();
            if (live != (UInt32)appid)
                throw WorkshopException.AppIdMismatch((UInt32)appid, live, operation, publishedFileId);
        }

        private IReadOnlyCollection<PublishedFileData> BuildItems()
        {
            ISteamUGC ugc = _client.GetISteamUGC()
                ?? throw new WorkshopException("Steamworks is not available (Steam not running or the ISteamUGC interface could not be created) for in-process workshop listing.");

            var items = new List<PublishedFileData>();
            UInt32 count = ugc.GetNumSubscribedItems();
            if (count == 0)
                return items;

            var ids = new UInt64[count];
            count = ugc.GetSubscribedItems(ids, count);
            for (UInt32 i = 0; i < count; i++)
            {
                UInt64 id = ids[i];
                EItemState state = ugc.GetItemState(id);

                var item = new PublishedFileData(id, state);
                if (state.HasFlag(EItemState.k_EItemStateInstalled))
                {
                    UInt64 sizeOnDisk = 0;
                    UInt32 timeStamp = 0;
                    var folder = new StringBuilder(260);
                    if (ugc.GetItemInstallInfo(id, ref sizeOnDisk, folder, (UInt32)folder.Capacity, ref timeStamp))
                    {
                        item.SizeOnDisk = sizeOnDisk;
                        item.Folder = folder.ToString();
                        item.TimeStamp = timeStamp;
                    }
                }

                if (state.HasFlag(EItemState.k_EItemStateNeedsUpdate))
                {
                    UInt64 bytesDownloaded = 0;
                    UInt64 bytesTotal = 0;
                    if (ugc.GetItemDownloadInfo(id, ref bytesDownloaded, ref bytesTotal))
                    {
                        item.BytesDownloaded = bytesDownloaded;
                        item.BytesTotal = bytesTotal;
                    }
                }

                items.Add(item);
            }
            return items;
        }

        private async Task<T> AwaitCallAsync<T>(ISteamUtils utils, UInt64 call, int k_iCallback, CancellationToken cancellationToken) where T : unmanaged
        {
            T? result = await TryAwaitCallAsync<T>(utils, call, k_iCallback, cancellationToken);
            return result ?? throw new WorkshopException("The Steam API call failed before completing.");
        }

        /// <summary>Like <see cref="AwaitCallAsync{T}"/> but returns null instead of throwing when the call fails, so a
        /// caller can inspect the live state and decide whether to recover (e.g. the download is still in flight).</summary>
        private async Task<T?> TryAwaitCallAsync<T>(ISteamUtils utils, UInt64 call, int k_iCallback, CancellationToken cancellationToken) where T : unmanaged
        {
            bool failed = false;
            while (!utils.IsAPICallCompleted(call, ref failed) && !failed)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(PollIntervalMs, cancellationToken);
            }

            if (failed)
                return null;

            int size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];
            GCHandle pinned = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr address = pinned.AddrOfPinnedObject();
                bool failedResult = false;
                if (!Steam.GetAPICallResult(_client.SteamPipe, call, address, size, k_iCallback, ref failedResult) || failedResult)
                    return null;
                return Marshal.PtrToStructure<T>(address);
            }
            finally
            {
                pinned.Free();
            }
        }

        private static bool IsInstalledAndCurrent(EItemState state)
            => state.HasFlag(EItemState.k_EItemStateInstalled) && !state.HasFlag(EItemState.k_EItemStateNeedsUpdate);

        private static bool IsDownloadActive(EItemState state)
            => state.HasFlag(EItemState.k_EItemStateDownloading) || state.HasFlag(EItemState.k_EItemStateDownloadPending);

        /// <summary>
        /// Captures the live Steam state of a workshop item for diagnostics after a failed call, mirroring the
        /// Bridge's messages. <c>GetItemState</c>/<c>GetItemDownloadInfo</c>/<c>GetItemInstallInfo</c> are
        /// synchronous local queries, so they keep working even when the async call itself reports a failure.
        /// </summary>
        private string DescribeItem(ulong publishedFileId)
        {
            ISteamUGC? ugc = _client.GetISteamUGC();
            if (ugc is null)
                return "state=[unavailable]";

            EItemState state = ugc.GetItemState(publishedFileId);
            StringBuilder info = new($"state=[{DescribeState(state)}]");

            UInt64 bytesDownloaded = 0, bytesTotal = 0;
            if (ugc.GetItemDownloadInfo(publishedFileId, ref bytesDownloaded, ref bytesTotal))
                info.Append($", download={bytesDownloaded}/{bytesTotal} bytes");

            UInt64 sizeOnDisk = 0;
            UInt32 timeStamp = 0;
            StringBuilder folder = new(1024);
            if (ugc.GetItemInstallInfo(publishedFileId, ref sizeOnDisk, folder, (UInt32)folder.Capacity, ref timeStamp))
                info.Append($", onDisk={sizeOnDisk} bytes, folder=\"{folder}\"");

            return info.ToString();
        }

        private static string DescribeState(EItemState state)
        {
            if (state == EItemState.k_EItemStateNone)
                return "none";

            List<string> parts = new();
            if (state.HasFlag(EItemState.k_EItemStateSubscribed)) parts.Add("Subscribed");
            if (state.HasFlag(EItemState.k_EItemStateLegacyItem)) parts.Add("LegacyItem");
            if (state.HasFlag(EItemState.k_EItemStateInstalled)) parts.Add("Installed");
            if (state.HasFlag(EItemState.k_EItemStateNeedsUpdate)) parts.Add("NeedsUpdate");
            if (state.HasFlag(EItemState.k_EItemStateDownloading)) parts.Add("Downloading");
            if (state.HasFlag(EItemState.k_EItemStateDownloadPending)) parts.Add("DownloadPending");
            if (state.HasFlag(EItemState.k_EItemStateDisabledLocally)) parts.Add("DisabledLocally");

            return parts.Count > 0 ? string.Join("|", parts) : state.ToString();
        }

        #endregion Helpers
    }
}