using System;
using System.IO;
using Xunit;

namespace SteamVent.Tests.InterProc
{
    /// <summary>
    /// Shared guard for the interproc Steam tests. Enforces the "no local appid override" model: if a
    /// <c>steam_appid.txt</c> is present in the test working directory it would silently bind the process to
    /// that appid and mask the environment-variable approach (see <see cref="SteamVent.Steamworks.SteamClient.AppId"/>),
    /// so the tests fail fast with a clear message instead of producing a misleading result.
    /// </summary>
    public static class SteamTestGuard
    {
        /// <summary>
        /// Fails the test if a local <c>steam_appid.txt</c> is present in the working directory (see type docs).
        /// Call this at the top of each interproc test.
        /// </summary>
        public static void EnsureNoAppIdOverride()
        {
            string appIdFile = Path.Combine(Directory.GetCurrentDirectory(), "steam_appid.txt");
            if (File.Exists(appIdFile))
                Assert.Fail(
                    $"steam_appid.txt was found at '{appIdFile}'. Remove it (or run the tests from a clean " +
                    "working directory): the SteamVent interproc tests establish the appid context via the " +
                    "SteamAppId environment variable (see SteamClient.AppId), and a local steam_appid.txt would " +
                    "override that and make the results non-deterministic.");
        }
    }
}