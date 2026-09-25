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
            if (!SteamProcessInfo.IsSteamInstalled)
                return;
            Assert.NotNull(LocalConfig.GetUserLocalConfigFile());
        }

        [Fact]
        public void GetClientAppIdsTest()
        {
            if (!SteamProcessInfo.IsSteamInstalled)
                return;
            Assert.NotEqual(0u, SteamProcessInfo.CurrentUserID);
        }
    }
}*/
