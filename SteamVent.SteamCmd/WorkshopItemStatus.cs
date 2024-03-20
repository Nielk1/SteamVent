using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public class WorkshopItemStatus
    {
        [Flags]
        public enum WorkshopDetectionType
        {
            Direct = 1,
            Cache = 2,
            Folder = 4,
            HtmlList = 8, // decorated by HTML scanning, normally just the fact it needs an update
        }

        public WorkshopItemStatus()
        {
        }

        public UInt64 WorkshopId { get; set; }
        public string Status { get; set; }
        public long Size { get; set; }
        public DateTime? DateTime { get; set; }
        public bool HasUpdate { get; set; }
        public bool Missing { get; set; }
        public WorkshopDetectionType Detection { get; set; }
    }
}
