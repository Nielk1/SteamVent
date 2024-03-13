using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public class WorkshopItemStatus
    {
        public enum WorkshopDetectionType
        {
            Direct,
            Cache,
            Folder,
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
