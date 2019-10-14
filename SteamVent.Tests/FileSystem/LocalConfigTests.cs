/*using NUnit.Framework;
using SteamVent.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Tests.FileSystem
{
    [TestFixture]
    public class LocalConfigTests
    {
        public static bool Is64Bit() { return IntPtr.Size == 8; }

        [Test]
        public void GetUserLocalConfigFileTest()
        {
            if (!SteamProcessInfo.IsSteamInstalled)
                Assert.Warn("Steam not installed");
            Assert.IsNotNull(LocalConfig.GetUserLocalConfigFile(), "Steam localconfig.vdf not found");
        }

        [Test]
        public void GetClientAppIdsTest()
        {
            if (!SteamProcessInfo.IsSteamInstalled)
                Assert.Warn("Steam not installed");
            Assert.NotZero(SteamProcessInfo.CurrentUserID, "Steam UserId is Zero, is Steam running?");
        }
    }
}*/
