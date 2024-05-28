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
        
        // TODO improve filesystem install check
        public bool GetAppInstalled(UInt32 appId)
        {
            return steamClient.GetSteamApps()?.GetAppInstalled(appId) ?? ((FileSystem.SteamApps.GetAppInstallDir(appId)?.Length ?? 0) > 0);
        }

        public string? GetAppInstallDir(UInt32 appId)
        {
            return steamClient.GetSteamApps()?.GetAppInstallDir(appId) ?? FileSystem.SteamApps.GetAppInstallDir(appId);
        }

        public string? GetAppLibraryDir(UInt32 appId)
        {
            string? InstallDir = GetAppInstallDir(appId);
            if (InstallDir != null)
            {
                var Paths = FileSystem.SteamProcessInfo.GetSteamLibraryPaths();
                foreach (string path in Paths)
                    if (!Path.GetRelativePath(path, InstallDir).Contains(".."))
                        return path;
            }
            return null;
        }
    }
}
