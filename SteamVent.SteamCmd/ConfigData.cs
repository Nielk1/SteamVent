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
        /// <summary>
        /// Optional value for SteamCmd's <c>+force_install_dir</c>, i.e. the directory SteamCmd
        /// should store its data (steamapps, etc.) in. When set, <c>+force_install_dir "path"</c>
        /// is forced to the start of the parameter list on every SteamCmd launch. When left
        /// null or empty the argument is not used at all.
        /// </summary>
        public string ForceInstallDir { get; set; }

        public string WorkshopStatusItem { get; set; }
        public string WorkshopDownloadItemError { get; set; }
        public string WorkshopDownloadItemSuccess { get; set; }

        public string[] BadStrings { get; set; }

        [JsonIgnore]
        public Regex RegWorkshopStatusItem { get; set; }
        [JsonIgnore]
        public Regex RegWorkshopDownloadItemError { get; set; }
        [JsonIgnore]
        public Regex RegWorkshopDownloadItemSuccess { get; set; }
    }
}
