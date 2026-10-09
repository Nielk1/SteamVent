using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.Steamworks;

namespace SteamVent.Bridge
{
    /// <summary>
    /// Adapts <see cref="BridgeContext"/> to the shared workshop contracts (<see cref="IWorkshopSource"/> and
    /// <see cref="IWorkshopCommands"/>), routing through the shared <see cref="BridgePool"/> so repeated work on the
    /// same appid reuses one warm child process (reaped after it goes idle) instead of spawning one per command.
    /// This assembly is optional: when it is absent the top-level context simply degrades to the other subsystems.
    /// </summary>
    public sealed class BridgeSource : IWorkshopSource, IWorkshopCommands
    {
        private readonly BridgePool _pool;

        /// <summary>Creates a source backed by the shared <see cref="BridgePool.Default"/>.</summary>
        public BridgeSource()
            : this(BridgePool.Default)
        {
        }

        /// <summary>Creates a source backed by a specific pool (mainly for testing).</summary>
        public BridgeSource(BridgePool pool)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        #region IWorkshopSource

        public Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => _pool.WithAsync((UInt32)appid, ctx => ctx.GetInstalledWorkshopItems(appid, cancellationToken));

        public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => _pool.WithAsync((UInt32)appid, ctx => ctx.GetSubscribedWorkshopItems(appid, cancellationToken));

        #endregion IWorkshopSource

        #region IWorkshopCommands

        public Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
            => _pool.WithAsync((UInt32)appid, ctx => ctx.SubscribeAsync(publishedFileId, appid, cancellationToken));

        public Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
            => _pool.WithAsync((UInt32)appid, ctx => ctx.UnsubscribeAsync(publishedFileId, appid, cancellationToken));

        public Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, IProgress<WorkshopItemProgress>? progress = null, CancellationToken cancellationToken = default)
            => _pool.WithAsync((UInt32)appid, ctx => ctx.DownloadAsync(publishedFileId, appid, progress, cancellationToken));

        #endregion IWorkshopCommands
    }
}