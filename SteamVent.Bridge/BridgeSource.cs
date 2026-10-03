using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.InterProc;

namespace SteamVent.Bridge
{
    /// <summary>
    /// Adapts <see cref="BridgeContext"/> to the SteamVent source contracts so the top-level context can
    /// use the Bridge for workshop status and for appid-context workshop commands. This assembly is
    /// optional: when it is absent, the library degrades to the built-in FileSystem/Steamworks sources
    /// instead of failing. Each operation opens a fresh Bridge child process scoped to the requested appid.
    /// </summary>
    public sealed class BridgeSource : ISource, IWorkshopSource, IWorkshopCommands, IDisposable
    {
        public string Name => "Bridge";
        public int Priority => 10; // the live, in-context source - preferred for workshop data
        public SteamSource Family => SteamSource.Bridge;

        public bool IsAvailable() => true; // availability is per-operation (it spawns the child process)
        public Task<bool> CheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

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