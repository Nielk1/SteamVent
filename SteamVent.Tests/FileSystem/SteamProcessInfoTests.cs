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
    public class SteamProcessInfoTests
    {
        public static bool Is64Bit() { return IntPtr.Size == 8; }

        [Test]
        public void SteamInstallPathTest()
        {
            var SteamInstallPath = SteamProcessInfo.SteamInstallPath;
            Assert.That(SteamInstallPath, Is.Not.Null.Or.Empty, "Steam install path not found");
            Assert.That(Directory.Exists(SteamInstallPath), Is.True, "Steam install path does not exist");
        }

        [Test]
        public void GetSteamLibraryPathsTest()
        {
            List<string> Paths = SteamProcessInfo.GetSteamLibraryPaths().ToList();
            Assert.That(Paths.Count, Is.GreaterThan(0), "No Steam library paths found");
            Assert.That(Paths[0], Is.EqualTo(SteamProcessInfo.SteamInstallPath), "First Steam library path is not the default path");
            foreach (string path in Paths)
            {
                Assert.That(Directory.Exists(path), Is.True, "Returned library path does not exist");
            }
        }

        [Test]
        public void SteamClientDllPathTest()
        {
            string SteamClientDllPath = SteamProcessInfo.SteamClientDllPath;
            Assert.AreEqual(Path.GetFileName(SteamClientDllPath), Is64Bit() ? "steamclient64.dll" : "steamclient.dll", "Unexpected SteamClientDllPath, got \"{0}\" expected \"{1}\"", Path.GetFileName(SteamClientDllPath), Is64Bit() ? "steamclient64.dll" : "steamclient.dll");
        }

        [Test]
        public void SteamExePathTest()
        {
            string SteamExePath = SteamProcessInfo.SteamExePath;
            Assert.AreEqual(Path.GetFileName(SteamExePath), "Steam.exe", "Unexpected Steam EXE Path, got \"{0}\" expected \"Steam.exe\"", Path.GetFileName(SteamExePath));
        }

        [Test]
        public void IsSteamInstalledTest()
        {
            Assert.IsTrue(SteamProcessInfo.IsSteamInstalled, "Steam is not installed");
        }

        [Test]
        public void GetSteamPidTest()
        {
            if (!SteamProcessInfo.IsSteamInstalled)
                Assert.Warn("Steam not installed");
            Assert.NotZero(SteamProcessInfo.GetSteamPid(), "Steam PID is Zero, is Steam running?");
        }

        [Test]
        public void GetSteamUserIdTest()
        {
            if (!SteamProcessInfo.IsSteamInstalled)
                Assert.Warn("Steam not installed");
            Assert.NotZero(SteamProcessInfo.CurrentUserID, "Steam UserId is Zero, is Steam running?");
        }

        [Test]
        public void SteamProcessTest()
        {
            Assert.IsNotNull(SteamProcessInfo.SteamProcess, "Steam Process not found");
        }
    }
}
