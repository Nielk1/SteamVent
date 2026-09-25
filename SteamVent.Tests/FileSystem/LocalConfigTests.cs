/*using SteamVent.FileSystem;
using System;
using Xunit;

namespace SteamVent.Tests.FileSystem
{
    public class LocalConfigTests
    {
        public static bool Is64Bit() { return IntPtr.Size == 8; }

        [Fact]
        public void GetUserLocalConfigFileTest()
        {
            Assert.SkipWhen(!SteamProcessInfo.IsSteamInstalled, "Steam not installed");
            Assert.NotNull(LocalConfig.GetUserLocalConfigFile(), "Steam localconfig.vdf not found");
        }

        [Fact]
        public void GetClientAppIdsTest()
        {
            Assert.SkipWhen(!SteamProcessInfo.IsSteamInstalled, "Steam not installed");
            Assert.NotEqual(0u, SteamProcessInfo.CurrentUserID, "Steam UserId is Zero, is Steam running?");
        }
    }
}*/
