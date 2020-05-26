using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public class ConfigData
    {
        public string WorkshopStatusItem { get; set; }
        public string WorkshopDownloadItemError { get; set; }
        public string WorkshopDownloadItemSuccess { get; set; }

        [JsonIgnore]
        public Regex RegWorkshopStatusItem { get; set; }
        [JsonIgnore]
        public Regex RegWorkshopDownloadItemError { get; set; }
        [JsonIgnore]
        public Regex RegWorkshopDownloadItemSuccess { get; set; }
    }
}
