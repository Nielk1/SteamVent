using SteamVent.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SteamVent.Tests.FileSystem
{
    public class SteamProcessInfoTests
    {
        private static bool Is64Bit() { return IntPtr.Size == 8; }

        [Fact]
        public void SteamInstallPathTest()
        {
            var SteamInstallPath = SteamProcessInfo.SteamInstallPath;
            Assert.NotNull(SteamInstallPath);//, "Steam install path not found");
            Assert.True(Directory.Exists(SteamInstallPath), "Steam install path does not exist");
        }

        [Fact]
        public void GetSteamLibraryPathsTest()
        {
            List<string> Paths = SteamProcessInfo.GetSteamLibraryPaths().ToList();
            Assert.True(Paths.Count > 0, "No Steam library paths found");
            //Assert.That(ComparePaths(Paths[0], SteamProcessInfo.SteamInstallPath), "First Steam library path is not the default path");
            Assert.True(Path.GetRelativePath(Paths[0], SteamProcessInfo.SteamInstallPath) == ".", "First Steam library path is not the default path");
            foreach (string path in Paths)
            {
                Assert.True(Directory.Exists(path), "Returned library path does not exist");
            }
        }

        [Fact]
        public void SteamClientDllPathTest()
        {
            string SteamClientDllPath = SteamProcessInfo.SteamClientDllPath;
            string DllName = Is64Bit() ? "steamclient64.dll" : "steamclient.dll";
            Assert.Equal(DllName, Path.GetFileName(SteamClientDllPath));//, $"Unexpected SteamClientDllPath, got \"{Path.GetFileName(SteamClientDllPath)}\" expected \"{DllName}\"");
        }

        [Fact]
        public void SteamExePathTest()
        {
            string SteamExePath = SteamProcessInfo.SteamExePath;
            //Assert.Equal("Steam.exe", Path.GetFileName(SteamExePath), "Unexpected Steam EXE Path, got \"{0}\" expected \"Steam.exe\"", Path.GetFileName(SteamExePath));
            Assert.True(Path.GetRelativePath(Path.GetFileName(SteamExePath), "Steam.exe") == ".", $"Unexpected Steam EXE Path, got \"{Path.GetFileName(SteamExePath)}\" expected \"Steam.exe\"");
        }

        [Fact]
        public void IsSteamInstalledTest()
        {
            Assert.True(SteamProcessInfo.IsSteamInstalled, "Steam is not installed");
        }

        [Fact]
        public void GetSteamPidTest()
        {
            Assert.SkipWhen(!SteamProcessInfo.IsSteamInstalled, "Steam not installed");
            Assert.NotEqual(0, SteamProcessInfo.GetSteamPid());//, "Steam PID is Zero, is Steam running?");
        }

        [Fact]
        public void GetSteamUserIdTest()
        {
            Assert.SkipWhen(!SteamProcessInfo.IsSteamInstalled, "Steam not installed");
            Assert.NotEqual(0u, SteamProcessInfo.CurrentUserID);//, "Steam UserId is Zero, is Steam running?");
        }

        [Fact]
        public void SteamProcessTest()
        {
            Assert.NotNull(SteamProcessInfo.SteamProcess);//, "Steam Process not found");
        }
    }
}
