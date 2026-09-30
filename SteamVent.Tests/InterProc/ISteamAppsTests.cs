using System;
using System.Collections.Generic;
using System.Reflection;
using SteamVent.InterProc;
using SteamVent.InterProc.Interfaces;
using Xunit;

namespace SteamVent.Tests.InterProc
{
    public class ISteamAppsTests
    {
        public static IEnumerable<object[]> SteamClientAndAppsVersions()
        {
            yield return new object[] { typeof(ISteamClient017), typeof(ISteamApps003) };
            yield return new object[] { typeof(ISteamClient017), typeof(ISteamApps004) };
            yield return new object[] { typeof(ISteamClient017), typeof(ISteamApps005) };
            yield return new object[] { typeof(ISteamClient017), typeof(ISteamApps006) };
            yield return new object[] { typeof(ISteamClient017), typeof(ISteamApps007) };
            yield return new object[] { typeof(ISteamClient017), typeof(ISteamApps008) };
        }

        [Theory]
        [MemberData(nameof(SteamClientAndAppsVersions))]
        public void BIsAppInstalledTest(Type SteamClientVersion, Type SteamAppsVersion)
        {
            Assert.SkipWhen(Attribute.IsDefined(SteamAppsVersion.GetMethod("BIsAppInstalled"), typeof(ObsoleteAttribute)), "BIsAppInstalled is marked obsolete / not implemented in this interface version");

            ISteamClient? SteamClient = null;
            Int32 Pipe = 0;
            Int32 User = 0;
            try
            {
                UInt32 InstalledAppID = UInt32.Parse(Core.Configuration["SteamAppsTests:InstalledAppID"]);
                Assert.True(InstalledAppID > 0);
                UInt32 UninstalledAppID = UInt32.Parse(Core.Configuration["SteamAppsTests:UninstalledAppID"]);
                Assert.True(UninstalledAppID > 0);

                Assert.True(Steam.Load(/*true*/));
                //SteamClient = Steam.CreateInterface<ISteamClient###>(IntPtr.Zero);
                SteamClient = (ISteamClient?)typeof(Steam)
                    ?.GetMethod("CreateInterface")
                    ?.MakeGenericMethod(new Type[] { SteamClientVersion })
                    ?.Invoke(null, new object[] { });
                Assert.NotNull(SteamClient);
                Pipe = SteamClient.CreateSteamPipe();
                Assert.True(Pipe > 0);
                User = SteamClient.ConnectToGlobalUser(Pipe);
                Assert.True(User > 0);
                //SteamApps = SteamClient.GetISteamApps<ISteamApps###>(User, Pipe);
                ISteamApps? SteamApps = (ISteamApps?)SteamClient.GetType()
                    ?.GetMethod("GetISteamApps")
                    ?.MakeGenericMethod(new Type[] { SteamAppsVersion })
                    ?.Invoke(SteamClient, new object[] { User, Pipe });
                Assert.NotNull(SteamApps);

                Assert.True(SteamApps.BIsAppInstalled(InstalledAppID));
                Assert.False(SteamApps.BIsAppInstalled(UninstalledAppID));
            }
            finally
            {
                if (SteamClient != null)
                {
                    SteamClient.ReleaseUser(Pipe, User);
                    SteamClient.BReleaseSteamPipe(Pipe);
                }
            }
        }
    }
}
