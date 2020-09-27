using NUnit.Framework;
using SteamVent.SteamCmd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Tests.SteamCmd
{
    [TestFixture]
    public class SteamCmdTests
    {
        private SteamCmdContext steamcmd;
        [SetUp]
        public void BaseSetUp()
        {
            steamcmd = SteamCmdContext.GetInstance();
            Assert.IsNotNull(steamcmd);
            steamcmd.Download();
        }

        [Test, Order(1)]
        public void WorkshopStatusTest()
        {
            List<WorkshopItemStatus> status = steamcmd.WorkshopStatus(624970);

            StringBuilder bld = new StringBuilder();
            bld.AppendLine("WorkshopId\tStatus   \tHasUpdate\tSize\tDateTime");
            foreach (var stat in status)
            {
                bld.AppendLine($"{stat.WorkshopId}\t{stat.Status}\t{stat.HasUpdate}    \t{stat.Size}\t{stat.DateTime}");
            }
            Assert.Pass(bld.ToString());
        }

        [Test, Order(2)]
        public void WorkshopDownloadItemTest()
        {
            string downloadString = steamcmd.WorkshopDownloadItem(624970, 1762479746);

            List<WorkshopItemStatus> status = steamcmd.WorkshopStatus(624970);
            Assert.Greater(status.Count, 0);
            Assert.IsTrue(status.Any(stat => stat.WorkshopId == 1762479746 && stat.Status == "installed"));

            Assert.Pass(downloadString);
        }
    }
}
