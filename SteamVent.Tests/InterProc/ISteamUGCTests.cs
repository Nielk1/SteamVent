using NUnit.Framework;
using System;
using SteamVent.InterProc;
using SteamVent.InterProc.Interfaces;
using System.Collections.Generic;
using System.Reflection;
using System.Configuration;
using NLog.Internal;

namespace SteamVent.Tests.InterProc
{
    //[TestFixture(typeof(ISteamClient016))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamUGC001))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamUGC002))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamUGC003))]
    [TestFixture(typeof(ISteamClient017), typeof(ISteamUGC005))]
    public class ISteamUGCTests
    {
        private ISteamClient? SteamClient { get; set; }
        Type _SteamClientVersion { get; set; }
        private Int32 Pipe { get; set; }
        private Int32 User { get; set; }
        private ISteamUGC? SteamUGC { get; set; }
        Type _SteamUGCVersion { get; set; }

        UInt64 PublishedFileID;
        public ISteamUGCTests(Type SteamClientVersion, Type SteamUGCVersion)
        {
            _SteamClientVersion = SteamClientVersion;
            _SteamUGCVersion = SteamUGCVersion;
            PublishedFileID = UInt32.Parse(Core.Configuration["PublishedFileID"]);
            Assert.Greater(PublishedFileID, 0);
        }

        [SetUp]
        public void BaseSetUp()
        {
            Assert.IsTrue(Steam.Load(/*true*/));
            //SteamClient = Steam.CreateInterface<ISteamClient###>(IntPtr.Zero);
            {
                SteamClient = (ISteamClient?)typeof(Steam)
                    ?.GetMethod("CreateInterface")
                    ?.MakeGenericMethod(new Type[] { _SteamClientVersion })
                    ?.Invoke(null, new object[] { });
            }
            Assert.IsNotNull(SteamClient);
            Pipe = SteamClient.CreateSteamPipe();
            Assert.Greater(Pipe, 0);
            User = SteamClient.ConnectToGlobalUser(Pipe);
            Assert.Greater(User, 0);
            //SteamUGC = SteamClient.GetISteamUGC<ISteamUGC###>(User, Pipe);
            {
                SteamUGC = (ISteamUGC?)SteamClient.GetType()
                    ?.GetMethod("GetISteamUGC")
                    ?.MakeGenericMethod(new Type[] { _SteamUGCVersion })
                    ?.Invoke(SteamClient, new object[] { User, Pipe });
            }
            Assert.IsNotNull(SteamUGC);
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
        public void GetItemDownloadInfoTest()
        {
            if (Attribute.IsDefined(_SteamUGCVersion.GetMethod("GetItemDownloadInfo"), typeof(ObsoleteAttribute)))
                Assert.Pass("Not Implemented in this Interface");
                //Assert.Ignore("Not Implemented in this Interface");
                //Assert.Inconclusive("Not Implemented in this Interface");

            Assert.IsNotNull(SteamUGC);

            UInt64 punBytesDownloaded = 0;
            UInt64 punBytesTotal = 0;
            bool retVal = SteamUGC.GetItemDownloadInfo(PublishedFileID, ref punBytesDownloaded, ref punBytesTotal);
            Assert.IsTrue(retVal);
        }
    }
}
