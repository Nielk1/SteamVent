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

        /// <summary>
        /// Regex patterns that match the terminal portion of a logical line that SteamCmd fails to
        /// newline-terminate, causing the next message to be concatenated onto it (e.g.
        /// "ERROR! Download item 123 failed (Access Denied).Unloading Steam API...OK"). When one of
        /// these patterns matches in the middle of a line, a CRLF is injected immediately after the
        /// match so the trailing text reads back in as its own line.
        /// </summary>
        public string[] UnterminatedLinePatterns { get; set; }

        [JsonIgnore]
        public Regex RegWorkshopStatusItem { get; set; }
        [JsonIgnore]
        public Regex RegWorkshopDownloadItemError { get; set; }
        [JsonIgnore]
        public Regex RegWorkshopDownloadItemSuccess { get; set; }
        [JsonIgnore]
        public Regex[] RegUnterminatedLinePatterns { get; set; }
    }
}
