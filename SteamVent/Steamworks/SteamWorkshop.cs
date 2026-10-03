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
            UInt64 call = ugc.SubscribeItem(publishedFileId);
            return AwaitCallAsync<RemoteStorageSubscribePublishedFileResult_t>(utils, call, RemoteStorageSubscribePublishedFileResult_t.k_iCallback, cancellationToken);
        }

        /// <summary>Unsubscribes from a workshop item, removing its installed content.</summary>
        public Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
        {
            var (ugc, utils) = RequireUgcAndUtils();
            UInt64 call = ugc.UnsubscribeItem(publishedFileId);
            return AwaitCallAsync<RemoteStorageUnsubscribePublishedFileResult_t>(utils, call, RemoteStorageUnsubscribePublishedFileResult_t.k_iCallback, cancellationToken);
        }

        /// <summary>Downloads (installs) a workshop item's content; the task completes when it is fully installed.</summary>
        public Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
        {
            var (ugc, utils) = RequireUgcAndUtils();
            UInt64 call = ugc.DownloadItem(publishedFileId);
            return AwaitCallAsync<DownloadItemResult_t>(utils, call, DownloadItemResult_t.k_iCallback, cancellationToken);
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
                throw new SteamException("Steamworks is not available (Steam not running or the ISteamUGC interface could not be created) for in-process workshop commands.");
            ISteamUtils? utils = _client.GetISteamUtils();
            if (utils == null)
                throw new SteamException("Steamworks ISteamUtils is not available for in-process workshop commands.");
            return (ugc, utils);
        }

        private IReadOnlyCollection<PublishedFileData> BuildItems()
        {
            ISteamUGC ugc = _client.GetISteamUGC()
                ?? throw new SteamException("Steamworks is not available (Steam not running or the ISteamUGC interface could not be created) for in-process workshop listing.");

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
            bool failed = false;
            while (!utils.IsAPICallCompleted(call, ref failed) && !failed)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(PollIntervalMs, cancellationToken);
            }

            if (failed)
                throw new SteamException("The Steam API call failed before completing.");

            int size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];
            GCHandle pinned = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr address = pinned.AddrOfPinnedObject();
                bool failedResult = false;
                if (!Steam.GetAPICallResult(_client.SteamPipe, call, address, size, k_iCallback, ref failedResult) || failedResult)
                    throw new SteamException($"Steam_GetAPICallResult for {typeof(T).Name} failed.");
                return Marshal.PtrToStructure<T>(address);
            }
            finally
            {
                pinned.Free();
            }
        }

        #endregion Helpers
    }
}