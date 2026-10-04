using System;
using SteamVent.Steamworks;
using SteamVent.Steamworks.Native.Interfaces;
using Xunit;

namespace SteamVent.Tests.InterProc
{
    /// <summary>
    /// Exercises <see cref="ISteamUtils.GetAppID"/> (the source of truth for the active appid) and the
    /// <see cref="SteamClient.AppId"/> property, which latches that value once the Steamworks context starts.
    /// Shares the <see cref="SteamInterProc"/> collection with the other interproc tests so they run serially
    /// (the underlying Steam client is a process-wide singleton), and the <see cref="SteamTestGuard"/> to fail
    /// fast if a local <c>steam_appid.txt</c> would override the environment-variable approach.
    /// </summary>
    [Collection("SteamInterProc")]
    public class ISteamUtilsTests
    {
        [Fact]
        public void GetAppID_IsLatchedOnClient_AfterContextStarts()
        {
            SteamTestGuard.EnsureNoAppIdOverride();
            using var client = SteamClient.Construct();
            Assert.True(client.EnsureSteamworks(), "Steam is not running / Steamworks could not be initialized.");

            ISteamUtils? utils = client.GetISteamUtils();
            Assert.NotNull(utils);

            UInt32 liveAppId = utils.GetAppID();

            // Once the context is active, AppId is latched to the live value reported by ISteamUtils.GetAppID.
            Assert.Equal((UInt32?)liveAppId, client.AppId);
        }

        [Fact]
        public void AppId_IsReadOnly_AfterContextStarts()
        {
            SteamTestGuard.EnsureNoAppIdOverride();
            using var client = SteamClient.Construct();
            Assert.True(client.EnsureSteamworks(), "Steam is not running / Steamworks could not be initialized.");

            // After the context has started, AppId is immutable (it reflects the live context, not a new request).
            Assert.Throws<InvalidOperationException>(() => client.AppId = 624970);
        }
    }
}