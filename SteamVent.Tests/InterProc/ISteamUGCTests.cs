// SteamUGC needs to be running under an appid it appears

//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using SteamVent.InterProc;
//using SteamVent.InterProc.Interfaces;
//using Xunit;
//
//namespace SteamVent.Tests.InterProc
//{
//    public class ISteamUGCTests
//    {
//        public static IEnumerable<object[]> SteamClientAndUgcVersions()
//        {
//            yield return new object[] { typeof(ISteamClient017), typeof(ISteamUGC001) };
//            yield return new object[] { typeof(ISteamClient017), typeof(ISteamUGC002) };
//            yield return new object[] { typeof(ISteamClient017), typeof(ISteamUGC003) };
//            yield return new object[] { typeof(ISteamClient017), typeof(ISteamUGC005) };
//        }
//
//        [Theory]
//        [MemberData(nameof(SteamClientAndUgcVersions))]
//        public void GetItemDownloadInfoTest(Type SteamClientVersion, Type SteamUGCVersion)
//        {
//            Assert.SkipWhen(Attribute.IsDefined(SteamUGCVersion.GetMethod("GetItemDownloadInfo"), typeof(ObsoleteAttribute)), "GetItemDownloadInfo is marked obsolete / not implemented in this interface version");
//
//            ISteamClient? SteamClient = null;
//            Int32 Pipe = 0;
//            Int32 User = 0;
//            try
//            {
//                UInt64 PublishedFileID = UInt32.Parse(Core.Configuration["PublishedFileID"]);
//                Assert.True(PublishedFileID > 0);
//
//                Assert.True(Steam.Load(/*true*/));
//                //SteamClient = Steam.CreateInterface<ISteamClient###>(IntPtr.Zero);
//                SteamClient = (ISteamClient?)typeof(Steam)
//                    ?.GetMethod("CreateInterface")
//                    ?.MakeGenericMethod(new Type[] { SteamClientVersion })
//                    ?.Invoke(null, new object[] { });
//                Assert.NotNull(SteamClient);
//                Pipe = SteamClient.CreateSteamPipe();
//                Assert.True(Pipe > 0);
//                User = SteamClient.ConnectToGlobalUser(Pipe);
//                Assert.True(User > 0);
//                //SteamUGC = SteamClient.GetISteamUGC<ISteamUGC###>(User, Pipe);
//                ISteamUGC? SteamUGC = (ISteamUGC?)SteamClient.GetType()
//                    ?.GetMethod("GetISteamUGC")
//                    ?.MakeGenericMethod(new Type[] { SteamUGCVersion })
//                    ?.Invoke(SteamClient, new object[] { User, Pipe });
//                Assert.NotNull(SteamUGC);
//
//                UInt64 punBytesDownloaded = 0;
//                UInt64 punBytesTotal = 0;
//                bool retVal = SteamUGC.GetItemDownloadInfo(PublishedFileID, ref punBytesDownloaded, ref punBytesTotal);
//                Assert.True(retVal);
//            }
//            finally
//            {
//                if (SteamClient != null)
//                {
//                    SteamClient.ReleaseUser(Pipe, User);
//                    SteamClient.BReleaseSteamPipe(Pipe);
//                }
//            }
//        }
//    }
//}
