using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.Steamworks;
using SteamVent.FileSystem;

namespace SteamVent
{
    /// <summary>
    /// The SteamVent <b>workshop</b> facade - a box over workshop reads and appid-context commands (subscribe /
    /// download / unsubscribe). Obtain it from <see cref="SteamContext.SteamWorkshop"/>. Each call is routed by
    /// the context to the best subsystem for the target appid (in-process Steamworks when eligible, else the
    /// Bridge, else SteamCmd, else the local filesystem) - you never see which module answered, only that it did
    /// so in accordance with the context's configuration.
    /// </summary>
    public sealed class SteamWorkshop : IWorkshopSource, IWorkshopCommands
    {
        private readonly SteamContext _context;

        internal SteamWorkshop(SteamContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>True when any enabled subsystem can serve workshop data / commands for this context.</summary>
        public bool IsAvailable
            => _context.Options.UseSteamworks || _context.Options.UseBridge || _context.Options.UseSteamCmd || _context.Options.UseFileSystem;

        #region IWorkshopCommands

        /// <summary>Subscribes to a workshop item for <paramref name="appid"/> via the best available subsystem.</summary>
        public Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
            => WorkshopSubscribeAsync(publishedFileId, (UInt32)appid, cancellationToken);

        /// <summary>Unsubscribes from a workshop item for <paramref name="appid"/> via the best available subsystem.</summary>
        public Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
            => WorkshopUnsubscribeAsync(publishedFileId, (UInt32)appid, cancellationToken);

        /// <summary>
        /// Downloads a workshop item for <paramref name="appid"/> via the best available subsystem, completing
        /// only when it is fully installed. Reports <paramref name="progress"/> samples while it transfers.
        /// </summary>
        public Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, IProgress<WorkshopItemProgress>? progress = null, CancellationToken cancellationToken = default)
            => WorkshopDownloadAsync(publishedFileId, (UInt32)appid, progress, cancellationToken);

        #endregion IWorkshopCommands

        #region IWorkshopSource

        /// <summary>Workshop items for <paramref name="appid"/> (in-process when eligible, else the other subsystems).</summary>
        public Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => GetWorkshopItems((UInt32)appid, cancellationToken);

        /// <summary>Subscribed workshop items for <paramref name="appid"/> (same surface as <see cref="GetInstalledWorkshopItems"/>).</summary>
        public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => GetInstalledWorkshopItems(appid, cancellationToken);

        #endregion IWorkshopSource

        #region Workshop reads (moved from SteamContext)

        /// <summary>
        /// Workshop items for <paramref name="appid"/>, in a deliberate per-call order: in-process Steamworks
        /// (when we are actually running under that app), else the Bridge, else SteamCmd, else the local
        /// filesystem. Each step is option-gated and degrades to the next when the subsystem is disabled or its
        /// assembly is excised.
        /// </summary>
        public Task<IReadOnlyCollection<PublishedFileData>> GetWorkshopItems(UInt32 appid, CancellationToken cancellationToken = default)
        {
            int appId = (int)appid;

            if (_context.Options.UseSteamworks && _context.UsesInProcessWorkshop(appid))
                return _context.InnerWorkshop.GetInstalledWorkshopItems(appId, cancellationToken);

            if (_context.Options.UseBridge && _context.TryBridgeWorkshop() is IWorkshopSource bridge)
                return bridge.GetInstalledWorkshopItems(appId, cancellationToken);

            if (_context.Options.UseSteamCmd && _context.TrySteamCmdWorkshop() is IWorkshopSource cmd)
                return cmd.GetInstalledWorkshopItems(appId, cancellationToken);

            // Final fallback: whatever is physically on disk under this app's workshop content dir.
            return Task.FromResult(GetWorkshopItemsFromDisk(appid));
        }

        /// <summary>Local workshop items physically present under <c>&lt;library&gt;/steamapps/workshop/content/{appid}</c>.</summary>
        internal static IReadOnlyCollection<PublishedFileData> GetWorkshopItemsFromDisk(UInt32 appid)
        {
            var result = new List<PublishedFileData>();
            var seen = new HashSet<ulong>();
            foreach (var library in SteamProcessInfo.GetSteamLibraryPaths())
            {
                if (string.IsNullOrEmpty(library)) continue;
                string contentDir = Path.Combine(library, "steamapps", "workshop", "content", appid.ToString());
                if (!Directory.Exists(contentDir)) continue;
                foreach (var dir in Directory.EnumerateDirectories(contentDir))
                {
                    if (!ulong.TryParse(Path.GetFileName(dir), out var id)) continue;
                    if (!seen.Add(id)) continue;
                    result.Add(new PublishedFileData(id, EItemState.k_EItemStateInstalled) { Folder = dir });
                }
            }
            return result;
        }

        /// <summary>Local workshop status (installed / subscribed, with sizes and update flags) for an appid.</summary>
        public Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(UInt32 appid, IProgress<double?>? progress = null, string? libraryPathOverride = null)
        {
            string? LibraryPath = libraryPathOverride ?? _context.SteamApps.GetAppLibrary(appid);
            if (LibraryPath == null)
                return Task.FromResult<List<WorkshopItemStatus>?>(null);

            return FileSystem.SteamWorkshop.WorkshopStatusAsync(LibraryPath, appid, progress);
        }

        #endregion Workshop reads

        #region Workshop commands (moved from SteamContext)

        /// <summary>
        /// The workshop command provider to use for <paramref name="appid"/>, in a deliberate order: in-process
        /// Steamworks when we are actually running under that app (fast, no child process), else the Bridge
        /// (default-off, optional assembly). SteamCmd does not provide appid-context commands, so it is not part
        /// of this path. Returns null when no provider applies, so callers can report the command as unavailable.
        /// </summary>
        public IWorkshopCommands? GetWorkshopCommands(UInt32 appid)
        {
            if (_context.Options.UseSteamworks && _context.UsesInProcessWorkshop(appid))
                return _context.InnerWorkshop;

            if (_context.Options.UseBridge)
                return _context.TryBridgeCommands();

            return null;
        }

        private IWorkshopCommands RequireWorkshopCommands(UInt32 appid)
            => GetWorkshopCommands(appid)
               ?? throw new SteamException($"No workshop command provider is available for appid {appid}. " +
                                           "Enable the Bridge, or enable Steamworks and set Options.SteamworksAppId to this appid.");

        /// <summary>Subscribes to a workshop item, routed in-process or via the Bridge for <paramref name="appid"/>.</summary>
        public Task<RemoteStorageSubscribePublishedFileResult_t> WorkshopSubscribeAsync(ulong publishedFileId, UInt32 appid, CancellationToken cancellationToken = default)
            => RequireWorkshopCommands(appid).SubscribeAsync(publishedFileId, (int)appid, cancellationToken);

        /// <summary>Unsubscribes from a workshop item, routed in-process or via the Bridge for <paramref name="appid"/>.</summary>
        public Task<RemoteStorageUnsubscribePublishedFileResult_t> WorkshopUnsubscribeAsync(ulong publishedFileId, UInt32 appid, CancellationToken cancellationToken = default)
            => RequireWorkshopCommands(appid).UnsubscribeAsync(publishedFileId, (int)appid, cancellationToken);

        /// <summary>
        /// Downloads a workshop item, routed in-process or via the Bridge for <paramref name="appid"/>, completing
        /// only when it is fully installed. Reports <paramref name="progress"/> samples while it transfers.
        /// </summary>
        public Task<DownloadItemResult_t> WorkshopDownloadAsync(ulong publishedFileId, UInt32 appid, IProgress<WorkshopItemProgress>? progress = null, CancellationToken cancellationToken = default)
            => RequireWorkshopCommands(appid).DownloadAsync(publishedFileId, (int)appid, progress, cancellationToken);

        #endregion Workshop commands
    }
}