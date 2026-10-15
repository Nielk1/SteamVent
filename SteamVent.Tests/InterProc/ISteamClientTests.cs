using System;
using System.Collections.Generic;
using System.Reflection;
using SteamVent.Steamworks.Native;
using SteamVent.Steamworks.Native.Interfaces;
using Xunit;

namespace SteamVent.Tests.InterProc
{
    [Collection("SteamInterProc")]
    public class ISteamClientTests
    {
        public static IEnumerable<object[]> Types_SteamClient_GetInterface(string functionName, Type[] types)
        {
            foreach (Type client in ISteamClient.Versions)
            {
                MethodInfo? m = client.GetMethod(functionName);
                if (m != null && !Attribute.IsDefined(m, typeof(ObsoleteAttribute)))
                {
                    foreach (Type t in types)
                    {
                        yield return new object[] { client, t };
                    }
                }
            }
        }

        public void SteamClientTest<T>(Type SteamClientVersion, Type SteamAppsVersion)
        {
            SteamTestGuard.EnsureNoAppIdOverride();

            ISteamClient? SteamClient = null;
            Int32 Pipe = 0;
            Int32 User = 0;
            try
            {
                Assert.True(Steam.Load(/*true*/));
                
                //SteamClient = Steam.CreateInterface<ISteamClient###>(IntPtr.Zero);
                SteamClient = (ISteamClient?)typeof(Steam)
                    ?.GetMethod("CreateInterface")
                    ?.MakeGenericMethod([SteamClientVersion])
                    ?.Invoke(null, []);
                Assert.NotNull(SteamClient);

                Pipe = SteamClient.CreateSteamPipe();
                Assert.True(Pipe > 0);

                User = SteamClient.ConnectToGlobalUser(Pipe);
                Assert.True(User > 0);

                //SteamSubInterface = SteamClient.GetI.....<ISteamApps###>(User, Pipe);
                T? SteamSubInterface = (T?)SteamClient.GetType()
                    ?.GetMethod("GetISteamApps")
                    ?.MakeGenericMethod([SteamAppsVersion])
                    ?.Invoke(SteamClient, [User, Pipe]);
                Assert.NotNull(SteamSubInterface);
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



        public static IEnumerable<object[]> Types_SteamClient_SteamApps() => Types_SteamClient_GetInterface("GetISteamApps", ISteamApps.Versions);

        [Theory]
        [MemberData(nameof(Types_SteamClient_SteamApps))]
        public void SteamClientSteamAppsTest(Type SteamClientVersion, Type SteamAppsVersion) => SteamClientTest<ISteamApps>(SteamClientVersion, SteamAppsVersion);
    }
}
