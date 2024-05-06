using SteamVent.InterProc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Steamworks
{
    public class SteamApps
    {
        SteamClient steamClient;
        public SteamApps(SteamClient steamClient)
        {
            this.steamClient = steamClient;
        }

        public string? GetAppInstallDir(uint appID)
        {
            if(steamClient.TryStartSteamworks())
            {
                var steamApps = steamClient.GetSteamApps();
                if (steamApps != null)
                {
                    return steamApps.GetAppInstallDir(appID);
                }
            }
            return null;
        }
    }
}
