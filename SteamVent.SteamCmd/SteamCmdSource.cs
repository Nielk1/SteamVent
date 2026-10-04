using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.Steamworks;

namespace SteamVent.SteamCmd
{
    /// <summary>
    /// Adapts the SteamCmd automation pipe to the shared workshop contract (<see cref="IWorkshopSource"/>).
    /// Optional: when this assembly is absent the top-level context simply degrades to the other subsystems.
    /// </summary>
    public sealed class SteamCmdSource : IWorkshopSource
    {
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