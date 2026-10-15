using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SteamVent.Steamworks.Native;
using SteamVent.Steamworks.Native.Interfaces;
using Xunit;
using Native = SteamVent.Steamworks.Native.Interfaces;

namespace SteamVent.Tests.InterProc
{
    /// <summary>
    /// Exercises the versioned <see cref="ISteamApps.BIsAppInstalled(UInt32)"/> interop.
    /// The test tree is grouped by the <see cref="ISteamClient"/> interface version used to obtain the
    /// <see cref="ISteamApps"/> interface: each nested class is one client interface (007-019) and its
    /// <c>BIsAppInstalled</c> theory enumerates the <see cref="ISteamApps"/> versions that actually implement the
    /// call (i.e. whose slot is not an <see cref="ObsoleteAttribute"/> stub). The app-version executions therefore
    /// nest under the client interface that spawned them, instead of a single flat list of every combination.
    /// </summary>
    public class ISteamAppsTests
    {
        /// <summary>
        /// Shared <c>BIsAppInstalled</c> behaviour for every client-version test class. <typeparamref name="TClient"/>
        /// is the concrete <see cref="ISteamClient"/> interface version bound by the nested class that derives from it.
        /// </summary>
        public abstract class Base<TClient> where TClient : SteamInterfaceWrapper, ISteamClient
        {
            /// <summary>
            /// The <see cref="ISteamApps"/> versions whose <c>BIsAppInstalled</c> slot is a real implementation
            /// (not an <see cref="ObsoleteAttribute"/> stub). Each is passed to the test as its short type name so
            /// the case label reads <c>ISteamApps005</c> rather than a fully-qualified name.
            /// </summary>
            public static IEnumerable<object[]> AppsVersions() =>
                ISteamApps.Versions
                    .Where(v => v.GetMethod("BIsAppInstalled") is { } m && !Attribute.IsDefined(m, typeof(ObsoleteAttribute)))
                    .Select(v => new object[] { v.Name });

            [Theory]
            [MemberData(nameof(AppsVersions))]
            public void BIsAppInstalled(string appsVersion)
            {
                SteamTestGuard.EnsureNoAppIdOverride();

                Type apps = ISteamApps.Versions.Single(v => v.Name == appsVersion);
                UInt32 installedAppId = UInt32.Parse(Core.Configuration["SteamAppsTests:InstalledAppID"]!);
                Assert.True(installedAppId > 0);
                UInt32 uninstalledAppId = UInt32.Parse(Core.Configuration["SteamAppsTests:UninstalledAppID"]!);
                Assert.True(uninstalledAppId > 0);

                ISteamClient? SteamClient = null;
                Int32 Pipe = 0;
                Int32 User = 0;
                try
                {
                    Assert.True(Steam.Load(/*true*/));

                    SteamClient = Steam.CreateInterface<TClient>();
                    Assert.NotNull(SteamClient);

                    Pipe = SteamClient.CreateSteamPipe();
                    Assert.True(Pipe > 0);

                    User = SteamClient.ConnectToGlobalUser(Pipe);
                    Assert.True(User > 0);

                    ISteamApps? SteamApps = (ISteamApps?)SteamClient
                        .GetType()
                        .GetMethod("GetISteamApps")!
                        .MakeGenericMethod([apps])
                        .Invoke(SteamClient, [User, Pipe]);
                    Assert.NotNull(SteamApps);

                    Assert.True(SteamApps!.BIsAppInstalled(installedAppId));
                    Assert.False(SteamApps.BIsAppInstalled(uninstalledAppId));
                }
                finally
                {
                    if (SteamClient is { } client)
                    {
                        client.ReleaseUser(Pipe, User);
                        client.BReleaseSteamPipe(Pipe);
                    }
                }
            }
        }

        [Collection("SteamInterProc")] public class ISteamClient007 : Base<Native.ISteamClient007> { }
        [Collection("SteamInterProc")] public class ISteamClient008 : Base<Native.ISteamClient008> { }
        [Collection("SteamInterProc")] public class ISteamClient009 : Base<Native.ISteamClient009> { }
        [Collection("SteamInterProc")] public class ISteamClient010 : Base<Native.ISteamClient010> { }
        [Collection("SteamInterProc")] public class ISteamClient011 : Base<Native.ISteamClient011> { }
        [Collection("SteamInterProc")] public class ISteamClient012 : Base<Native.ISteamClient012> { }
        [Collection("SteamInterProc")] public class ISteamClient013 : Base<Native.ISteamClient013> { }
        [Collection("SteamInterProc")] public class ISteamClient014 : Base<Native.ISteamClient014> { }
        [Collection("SteamInterProc")] public class ISteamClient015 : Base<Native.ISteamClient015> { }
        [Collection("SteamInterProc")] public class ISteamClient016 : Base<Native.ISteamClient016> { }
        [Collection("SteamInterProc")] public class ISteamClient017 : Base<Native.ISteamClient017> { }
        [Collection("SteamInterProc")] public class ISteamClient018 : Base<Native.ISteamClient018> { }
        [Collection("SteamInterProc")] public class ISteamClient019 : Base<Native.ISteamClient019> { }
    }
}
