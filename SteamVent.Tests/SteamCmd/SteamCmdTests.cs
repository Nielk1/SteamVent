/*using SteamVent.SteamCmd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SteamVent.Tests.SteamCmd
{
    public class SteamCmdTests
    {
        private SteamCmdContext steamcmd;
        public SteamCmdTests()
        {
            steamcmd = SteamCmdContext.Instance;
            Assert.NotNull(steamcmd);
            steamcmd.DownloadAsync().Wait();
        }

        [Fact]
        public void WorkshopStatusTest()
        {
            List<WorkshopItemStatus> status = steamcmd.WorkshopStatusAsync(624970).Result;

            StringBuilder bld = new StringBuilder();
            bld.AppendLine("WorkshopId\tStatus   \tHasUpdate\tSize\tDateTime");
            foreach (var stat in status)
            {
                bld.AppendLine($"{stat.WorkshopId}\t{stat.Status}\t{stat.HasUpdate}    \t{stat.Size}\t{stat.DateTime}");
            }
            return; // output printed for manual inspection
        }

        [Fact]
        public void WorkshopDownloadItemTest()
        {
            string downloadString = steamcmd.WorkshopDownloadItemAsync(624970, 1762479746).Result;

            List<WorkshopItemStatus> status = steamcmd.WorkshopStatusAsync(624970).Result;
            Assert.True(status.Count > 0);
            Assert.True(status.Any(stat => stat.WorkshopId == 1762479746 && stat.Status == "installed"));

            return; // downloadString printed for manual inspection
        }
    }
}
*/