using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.Steamworks;

namespace SteamVent
{
    /// <summary>
    /// The SteamVent <b>workshop</b> facade - a box over workshop reads and appid-context commands
    /// (subscribe / download / unsubscribe). Obtain it from <see cref="SteamContext.SteamWorkshop"/> rather than
    /// constructing it yourself: the context owns the underlying <see cref="Steamworks.SteamClient"/> and routes
    /// every call to the best configured source - in-process <see cref="Steamworks.SteamWorkshop"/> when this
    /// process is running under the target app, else the Bridge, else SteamCmd, else the local FileSystem
    /// fallback. You never see which module answered - only that it did so in accordance with the context's
    /// configuration.
    ///
    /// The facade implements the shared <see cref="IWorkshopSource"/> and <see cref="IWorkshopCommands"/>
    /// contracts so it can stand in for a raw source anywhere one is expected.
    ///
    /// If you specifically want the raw child implementation, use <c>Steamworks.SteamWorkshop</c> over a
    /// <c>Steamworks.SteamClient</c> you construct yourself.
    /// </summary>
    public sealed class SteamWorkshop : IWorkshopSource, IWorkshopCommands
    {
        private readonly SteamContext _context;
        private readonly Steamworks.SteamWorkshop _inner;

        internal SteamWorkshop(SteamContext context, Steamworks.SteamWorkshop inner)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <summary>True when any configured source can serve workshop data / commands for this context.</summary>
        public bool IsAvailable
            => _context.Options.Sources.HasFlag(SteamSource.Steamworks)
            || _context.Options.Sources.HasFlag(SteamSource.Bridge)
            || _context.Options.Sources.HasFlag(SteamSource.SteamCmd)
            || _context.Options.Sources.HasFlag(SteamSource.FileSystem);

        #region IWorkshopCommands

        /// <summary>Subscribes to a workshop item, routed to the best configured source for <paramref name="appid"/>.</summary>
        public Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
            => RequireCommands(appid).SubscribeAsync(publishedFileId, appid, cancellationToken);

        /// <summary>Unsubscribes from a workshop item, routed to the best configured source for <paramref name="appid"/>.</summary>
        public Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
            => RequireCommands(appid).UnsubscribeAsync(publishedFileId, appid, cancellationToken);

        /// <summary>Downloads a workshop item, routed to the best configured source for <paramref name="appid"/>.</summary>
        public Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
            => RequireCommands(appid).DownloadAsync(publishedFileId, appid, cancellationToken);

        #endregion IWorkshopCommands

        #region IWorkshopSource

        /// <summary>Workshop items for <paramref name="appid"/> - in-process when eligible, else merged across the configured sources.</summary>
        public Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
        {
            if (_context.UsesInProcessWorkshop((UInt32)appid))
                return _inner.GetInstalledWorkshopItems(appid, cancellationToken);
            return _context.GetWorkshopItemsAsync(appid, cancellationToken);
        }

        /// <summary>Subscribed workshop items for <paramref name="appid"/> (same surface as <see cref="GetInstalledWorkshopItems"/>, per the contract).</summary>
        public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => GetInstalledWorkshopItems(appid, cancellationToken);

        #endregion IWorkshopSource

        /// <summary>Local workshop status (installed / subscribed, with sizes and update flags) for an appid.</summary>
        public Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(UInt32 appid, IProgress<double?>? progress = null, string? libraryPathOverride = null)
            => _context.WorkshopStatusAsync(appid, progress, libraryPathOverride);

        private IWorkshopCommands RequireCommands(int appid)
            => _context.GetWorkshopCommands((UInt32)appid)
               ?? throw new SteamException($"No workshop command provider is available for appid {appid}. " +
                                           "Enable the Bridge, or enable Steamworks and set Options.SteamworksAppId to this appid.");
    }
}