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
    /// Exercises the <see cref="ISteamClient.GetISteamApps{TInterface}(Int32, Int32)"/> factory slot across every
    /// <see cref="ISteamApps"/> version. Like <see cref="ISteamAppsTests"/>, the tree is grouped by the
    /// <see cref="ISteamClient"/> interface version used to make the call: each nested class is one client interface
    /// (007-019) and its <c>GetISteamApps</c> theory enumerates the <see cref="ISteamApps"/> versions it is invoked
    /// with, so the app-version executions nest under the client interface that spawned them.
    /// </summary>
    public class ISteamClientTests
    {
        /// <summary>
        /// Shared <c>GetISteamApps</c> behaviour for every client-version test class. <typeparamref name="TClient"/>
        /// is the concrete <see cref="ISteamClient"/> interface version bound by the nested class that derives from it.
        /// </summary>
        public abstract class Base<TClient> where TClient : SteamInterfaceWrapper, ISteamClient
        {
            /// <summary>
            /// Every <see cref="ISteamApps"/> version (the <c>GetISteamApps</c> factory is exercised against each of
            /// them). Each is passed to the test as its short type name so the case label reads <c>ISteamApps001</c>
            /// rather than a fully-qualified name.
            /// </summary>
            public static IEnumerable<object[]> AppsVersions() =>
                ISteamApps.Versions.Select(v => new object[] { v.Name });

            [Theory]
            [MemberData(nameof(AppsVersions))]
            public void GetISteamApps(string appsVersion)
            {
                SteamTestGuard.EnsureNoAppIdOverride();

                Type apps = ISteamApps.Versions.Single(v => v.Name == appsVersion);

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
