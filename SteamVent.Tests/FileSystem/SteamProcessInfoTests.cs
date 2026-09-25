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
            Assert.NotNull(SteamInstallPath);
            Assert.True(Directory.Exists(SteamInstallPath));
        }

        [Fact]
        public void GetSteamLibraryPathsTest()
        {
            List<string> Paths = SteamProcessInfo.GetSteamLibraryPaths().ToList();
            Assert.True(Paths.Count > 0);
            //Assert.That(ComparePaths(Paths[0], SteamProcessInfo.SteamInstallPath), "First Steam library path is not the default path");
            Assert.True(Path.GetRelativePath(Paths[0], SteamProcessInfo.SteamInstallPath) == ".");
            foreach (string path in Paths)
            {
                Assert.True(Directory.Exists(path));
            }
        }

        [Fact]
        public void SteamClientDllPathTest()
        {
            string SteamClientDllPath = SteamProcessInfo.SteamClientDllPath;
            string DllName = Is64Bit() ? "steamclient64.dll" : "steamclient.dll";
            Assert.Equal(DllName, Path.GetFileName(SteamClientDllPath));
        }

        [Fact]
        public void SteamExePathTest()
        {
            string SteamExePath = SteamProcessInfo.SteamExePath;
            //Assert.Equal("Steam.exe", Path.GetFileName(SteamExePath), "Unexpected Steam EXE Path, got \"{0}\" expected \"Steam.exe\"", Path.GetFileName(SteamExePath));
            Assert.True(Path.GetRelativePath(Path.GetFileName(SteamExePath), "Steam.exe") == ".");
        }

        [Fact]
        public void IsSteamInstalledTest()
        {
            Assert.True(SteamProcessInfo.IsSteamInstalled);
        }

        [Fact]
        public void GetSteamPidTest()
        {
            if (!SteamProcessInfo.IsSteamInstalled)
                return;
            Assert.NotEqual(0, SteamProcessInfo.GetSteamPid());
        }

        [Fact]
        public void GetSteamUserIdTest()
        {
            if (!SteamProcessInfo.IsSteamInstalled)
                return;
            Assert.NotEqual(0u, SteamProcessInfo.CurrentUserID);
        }

        [Fact]
        public void SteamProcessTest()
        {
            Assert.NotNull(SteamProcessInfo.SteamProcess);
        }
    }
}
