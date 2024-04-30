using NUnit.Framework;
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
    public class SteamAppsTests
    {
        UInt32 InstalledAppID;
        public SteamAppsTests()
        {
            InstalledAppID = UInt32.Parse(Core.Configuration["InstalledAppID"]);
            Assert.Greater(InstalledAppID, 0);
        }

        [Test]
        public void GetAppInstallDirTest()
        {
            string? AppInstallPath = SteamVent.FileSystem.SteamApps.GetAppInstallDir(InstalledAppID);
            Assert.That(AppInstallPath, Is.Not.Null.Or.Empty, "App install path not found");
            Assert.That(Directory.Exists(AppInstallPath), Is.True, "App install path does not exist");
        }
    }
}
