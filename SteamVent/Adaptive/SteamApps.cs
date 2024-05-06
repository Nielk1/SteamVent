using SteamVent.Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Adaptive
{
    public class SteamApps
    {
        SteamContext context;
        Steamworks.SteamClient steamClient;
        public SteamApps(SteamContext context, Steamworks.SteamClient steamClient)
        {
            this.context = context;
            this.steamClient = steamClient;
        }

        public string? GetAppInstallDir(UInt32 appID)
        {
            return steamClient.GetSteamApps()?.GetAppInstallDir(appID) ?? FileSystem.SteamApps.GetAppInstallDir(appID);
        }
    }
}
