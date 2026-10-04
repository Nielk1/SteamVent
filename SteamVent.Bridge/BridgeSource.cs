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
    /// <see cref="IWorkshopCommands"/>). This assembly is optional: when it is absent the top-level context
    /// simply degrades to the other subsystems instead of failing. Each operation opens a fresh Bridge child
    /// process scoped to the requested appid.
    /// </summary>
    public sealed class BridgeSource : IWorkshopSource, IWorkshopCommands, IDisposable
    {
        #region IWorkshopSource

        public async Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
        {
            using var bridge = new BridgeContext((UInt32)appid);
            return await bridge.SteamWorkshopListAsync();
        }

        public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => GetInstalledWorkshopItems(appid, cancellationToken);

        #endregion IWorkshopSource

        #region IWorkshopCommands

        public async Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
        {
            using var bridge = new BridgeContext((UInt32)appid);
            return await bridge.SteamWorkshopSubscribeAsync(publishedFileId);
        }

        public async Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
        {
            using var bridge = new BridgeContext((UInt32)appid);
            return await bridge.SteamWorkshopUnsubscribeAsync(publishedFileId);
        }

        public async Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
        {
            using var bridge = new BridgeContext((UInt32)appid);
            return await bridge.SteamWorkshopDownloadAsync(publishedFileId);
        }

        #endregion IWorkshopCommands

        public void Dispose() { /* BridgeContext instances are scoped to each operation */ }
    }
}