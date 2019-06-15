using NUnit.Framework;
using System;
using SteamVent.InterProc;
using SteamVent.InterProc.Interfaces;
using System.Collections.Generic;
using System.Reflection;
using System.Configuration;

namespace SteamVent.Tests.InterProc
{
    //[TestFixture(typeof(ISteamClient016))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamApps003))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamApps004))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamApps005))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamApps006))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamApps007))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamApps008))]
    public class ISteamAppsTests
    {
        private ISteamClient SteamClient { get; set; }
        Type _SteamClientVersion { get; set; }
        private Int32 Pipe { get; set; }
        private Int32 User { get; set; }
        private ISteamApps SteamApps { get; set; }
        Type _SteamAppsVersion { get; set; }

        UInt32 InstalledAppID;
        UInt32 UninstalledAppID;
        public ISteamAppsTests(Type SteamClientVersion, Type SteamAppsVersion)
        {
            _SteamClientVersion = SteamClientVersion;
            _SteamAppsVersion = SteamAppsVersion;

            InstalledAppID = UInt32.Parse(ConfigurationManager.AppSettings["InstalledAppID"]);
            Assert.Greater(InstalledAppID, 0);
            UninstalledAppID = UInt32.Parse(ConfigurationManager.AppSettings["UninstalledAppID"]);
            Assert.Greater(UninstalledAppID, 0);
        }

        [SetUp]
        public void BaseSetUp()
        {
            Assert.IsTrue(Steam.Load(true));
            //SteamClient = Steam.CreateInterface<ISteamClient###>(IntPtr.Zero);
            {
                SteamClient = (ISteamClient)typeof(Steam)
                    .GetMethod("CreateInterface")
                    .MakeGenericMethod(new Type[] { _SteamClientVersion })
                    .Invoke(null, new object[] { });
            }
            Assert.IsNotNull(SteamClient);
            Pipe = SteamClient.CreateSteamPipe();
            Assert.Greater(Pipe, 0);
            User = SteamClient.ConnectToGlobalUser(Pipe);
            Assert.Greater(User, 0);
            //SteamApps = SteamClient.GetISteamApps<ISteamApps###>(User, Pipe);
            {
                SteamApps = (ISteamApps)SteamClient.GetType()
                    .GetMethod("GetISteamApps")
                    .MakeGenericMethod(new Type[] { _SteamAppsVersion })
                    .Invoke(SteamClient, new object[] { User, Pipe });
            }
            Assert.IsNotNull(SteamApps);
        }

        [TearDown]
        public void BaseTearDown()
        {
            if (SteamClient == null)
                return;

            SteamClient.ReleaseUser(Pipe, User);
            SteamClient.BReleaseSteamPipe(Pipe);
        }

        [Test]
        public void BIsAppInstalledTest()
        {
            if (Attribute.IsDefined(_SteamAppsVersion.GetMethod("BIsAppInstalled"), typeof(ObsoleteAttribute)))
                Assert.Ignore();

            Assert.IsTrue(SteamApps.BIsAppInstalled(InstalledAppID));
            Assert.IsFalse(SteamApps.BIsAppInstalled(UninstalledAppID));
        }
    }
}
