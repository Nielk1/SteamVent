using SteamVent.FileSystem;
using System;
using System.IO;
using Xunit;

namespace SteamVent.Tests.FileSystem
{
    public class SteamAppsTests
    {
        UInt32 InstalledAppID;
        public SteamAppsTests()
        {
            InstalledAppID = UInt32.Parse(Core.Configuration["InstalledAppID"]);
            Assert.True(InstalledAppID > 0);
        }

        [Fact]
        public void GetAppInstallDirTest()
        {
            string? AppInstallPath = SteamVent.FileSystem.SteamApps.GetAppInstallDir(InstalledAppID);
            Assert.NotNull(AppInstallPath);//, "App install path not found");
            Assert.True(Directory.Exists(AppInstallPath), "App install path does not exist");
        }
    }
}
