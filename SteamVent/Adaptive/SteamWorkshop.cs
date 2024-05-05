using SteamVent.Common;
using SteamVent.Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Adaptive
{
    public class SteamWorkshop
    {
        SteamContext context;
        Steamworks.SteamClient steamClient;
        public SteamWorkshop(SteamContext context, Steamworks.SteamClient steamClient)
        {
            this.context = context;
            this.steamClient = steamClient;
        }

        public async Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(string LibraryPathOverride, UInt32 AppId, IProgress<double?>? Progress = null)
        {
            string? LibraryPath = LibraryPathOverride ?? context.GetSteamApps().GetAppInstallDir(AppId);
            if (LibraryPath == null)
                return null;
            return await FileSystem.SteamWorkshop.WorkshopStatusAsync(LibraryPath, AppId, Progress);
        }
    }
}
