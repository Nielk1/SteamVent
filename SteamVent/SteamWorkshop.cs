using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.Steamworks;

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
            => _context.WorkshopSubscribeAsync(publishedFileId, (UInt32)appid, cancellationToken);

        /// <summary>Unsubscribes from a workshop item for <paramref name="appid"/> via the best available subsystem.</summary>
        public Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
            => _context.WorkshopUnsubscribeAsync(publishedFileId, (UInt32)appid, cancellationToken);

        /// <summary>Downloads a workshop item for <paramref name="appid"/> via the best available subsystem.</summary>
        public Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
            => _context.WorkshopDownloadAsync(publishedFileId, (UInt32)appid, cancellationToken);

        #endregion IWorkshopCommands

        #region IWorkshopSource

        /// <summary>Workshop items for <paramref name="appid"/> (in-process when eligible, else the other subsystems).</summary>
        public Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => _context.GetWorkshopItems((UInt32)appid, cancellationToken);

        /// <summary>Subscribed workshop items for <paramref name="appid"/> (same surface as <see cref="GetInstalledWorkshopItems"/>).</summary>
        public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => GetInstalledWorkshopItems(appid, cancellationToken);

        #endregion IWorkshopSource

        /// <summary>Local workshop status (installed / subscribed, with sizes and update flags) for an appid.</summary>
        public Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(UInt32 appid, IProgress<double?>? progress = null, string? libraryPathOverride = null)
            => _context.WorkshopStatusAsync(appid, progress, libraryPathOverride);
    }
}