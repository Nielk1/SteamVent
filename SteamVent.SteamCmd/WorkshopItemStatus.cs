using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public class WorkshopItemStatus : IEquatable<WorkshopItemStatus>
    {
        private string key;

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

        // Rare data from HTTP
        public string Title { get; set; }
        public string Image { get; set; }

        public bool Equals(WorkshopItemStatus? other)
        {
            return this?.WorkshopId == other?.WorkshopId
                && this?.Status == other?.Status
                && this?.Size == other?.Size
                && this?.DateTime == other?.DateTime
                && this?.HasUpdate == other?.HasUpdate
                && this?.Missing == other?.Missing
                && this?.Detection == other?.Detection
                && this?.Title == other?.Title
                && this?.Image == other?.Image;
        }
    }
}
