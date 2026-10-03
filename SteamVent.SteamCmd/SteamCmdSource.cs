using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.InterProc;

namespace SteamVent.SteamCmd
{
    /// <summary>
    /// Adapts the SteamCmd pipeline to the SteamVent source contracts. SteamCmd enumerates workshop state
    /// via its own process; this maps that to the shared <see cref="PublishedFileData"/> model. Optional:
    /// when this assembly is absent the library simply degrades to the built-in sources.
    /// </summary>
    public sealed class SteamCmdSource : ISource, IWorkshopSource
    {
        public string Name => "SteamCmd";
        public int Priority => 20; // between the live Bridge and the local FileSystem fallback
        public SteamSource Family => SteamSource.SteamCmd;

        public bool IsAvailable()
        {
            try { return SteamCmdContext.Instance != null; }
            catch { return false; }
        }
        public Task<bool> CheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(IsAvailable());

        public async Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
        {
            var status = await SteamCmdContext.Instance.WorkshopStatusAsync((UInt32)appid, null, null);
            var result = new List<PublishedFileData>();
            if (status == null) return result;
            foreach (var item in status)
            {
                EItemState state = item.Missing ? EItemState.k_EItemStateSubscribed : EItemState.k_EItemStateInstalled;
                if (item.HasUpdate) state |= EItemState.k_EItemStateNeedsUpdate;
                result.Add(new PublishedFileData(item.WorkshopId, state)
                {
                    SizeOnDisk = item.Size >= 0 ? (ulong)item.Size : 0,
                });
            }
            return result;
        }

        public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => GetInstalledWorkshopItems(appid, cancellationToken);
    }
}