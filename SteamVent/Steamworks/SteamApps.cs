using SteamVent.Steamworks.Native.Interfaces;
using System;

namespace SteamVent.Steamworks
{
    /// <summary>
    /// Thin, synchronous wrapper over the Steamworks <see cref="ISteamApps"/> interop: one method
    /// maps to one native call. Every method returns <c>null</c> when Steamworks is not available
    /// (Steam not running, or the interface could not be created) so callers can degrade gracefully.
    /// </summary>
    public class SteamApps
    {
        SteamClient steamClient;
        public SteamApps(SteamClient steamClient)
        {
            this.steamClient = steamClient;
        }

        /// <summary>Install directory for the app, or null when it is not installed / Steamworks unavailable.</summary>
        public string? GetAppInstallDir(uint appID)
        {
            var apps = steamClient.GetISteamApps();
            if (apps == null) return null;
            string dir = apps.GetAppInstallDir(appID);
            return string.IsNullOrEmpty(dir) ? null : dir;
        }

        /// <summary>Whether the app is installed; null when Steamworks is unavailable.</summary>
        public bool? GetAppInstalled(uint appID)
        {
            var apps = steamClient.GetISteamApps();
            return apps == null ? null : apps.BIsAppInstalled(appID);
        }

        /// <summary>Whether the current user owns (is subscribed to) the app; null when Steamworks is unavailable.</summary>
        public bool? IsSubscribed(uint appID)
        {
            var apps = steamClient.GetISteamApps();
            return apps == null ? null : apps.BIsSubscribedApp(appID);
        }
    }
}
