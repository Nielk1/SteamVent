using Gameloop.Vdf.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.HashFunction;
using System.Data.HashFunction.CRC;

namespace SteamVent
{
    public abstract class SteamLaunchable
    {
        public enum SteamLaunchableType
        {
            App = 0,
            GameMod = 1,
            Shortcut = 2,
            P2P = 3
        }

        public UInt32 AppID { get; set; }
        public abstract SteamLaunchableType ShortcutType { get; }
        //public string ShortcutData { get; set; }

        public abstract string Title { get; }
        public abstract string Icon { get; }
        public abstract string AppType { get; }



        private static readonly ICRCConfig crcSetting = new CRCConfig()
        {
            HashSizeInBits = 32,
            Polynomial = 0x04C11DB7,
            InitialValue = 0xffffffff,
            ReflectIn = true,
            ReflectOut = true,
            XOrOut = 0xffffffff,
        };

        public abstract string ModIdString { get; }

        public virtual UInt32 GenerateModID()
        {
            ICRC algorithm = CRCFactory.Instance.Create(crcSetting);
            string crc_input = ModIdString;
            return BitConverter.ToUInt32(algorithm.ComputeHash(Encoding.UTF8.GetBytes(crc_input).Select(dr => (byte)dr).ToArray()).Hash, 0) | 0x80000000;
        }

        public UInt64 GetShortcutID()
        {
            UInt64 high_32 = (((UInt64)GenerateModID()));
            UInt64 full_64 = ((high_32 << 32) | ((UInt32)ShortcutType << 24) | AppID);
            return full_64;
        }
    }

    public abstract class SteamLaunchableMod : SteamLaunchable
    {
        public override SteamLaunchableType ShortcutType { get { return SteamLaunchableType.GameMod; } }
    }

    public class SteamLaunchableModGoldSrc : SteamLaunchableMod
    {
        public Dictionary<string, string> ConfigLines { get; set; }
        public string ModFolder { get; set; }
        public string ModPath { get; set; }
        public string ModTitle { get; set; }

        public override string Title { get { return ModTitle; } }
        public override string Icon { get { return null; } }
        public override string AppType { get { return "GoldSrc Mod"; } }
        public override string ModIdString { get { return ModFolder; } }

        public SteamLaunchableModGoldSrc(uint AppID, string ModFolder, string ModTitle)
        {
            this.AppID = AppID;
            this.ModFolder = ModFolder;
            this.ModTitle = ModTitle;
            this.ConfigLines = new Dictionary<string, string>();
        }

        private static Dictionary<string, string> ProcLibListGamFile(string liblistFilePath)
        {
            string lines = File.ReadAllText(liblistFilePath);
            Dictionary<string, string> ConfigLines = new Dictionary<string, string>();
            var matches = Regex.Matches(lines, "\\s*(\\w+)\\s+\\\"(.*)\\\"", RegexOptions.IgnoreCase & RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value.ToLowerInvariant();
                string value = match.Groups[2].Value;
                if (!ConfigLines.ContainsKey(key)) ConfigLines.Add(key, value);
            }
            return ConfigLines;
        }

        public static SteamLaunchableModGoldSrc Make(UInt32 HostAppID, string ModPath, string liblistFilePath)
        {
            Dictionary<string, string> ConfigLines = ProcLibListGamFile(liblistFilePath);

            string gameDir = ConfigLines.ContainsKey("gamedir") ? ConfigLines["gamedir"] : Path.GetFileName(ModPath);
            string gameTitle = ConfigLines.ContainsKey("game") ? ConfigLines["game"] : gameDir;
            SteamLaunchableModGoldSrc src = new SteamLaunchableModGoldSrc(HostAppID, gameDir, gameTitle)
            {
                ConfigLines = ConfigLines,
                ModPath = ModPath
            };

            return src;
        }
    }

    public class SteamLaunchableModSource : SteamLaunchableMod
    {
        public string ModFolder { get; set; }
        public string ModTitle { get; set; }
        public string ModIcon { get; set; }
        public string ModDir { get; set; }

        public override string Title { get { return ModTitle; } }
        public override string Icon { get { return ModIcon != null ? Path.Combine(ModDir, ModIcon + ".ico") : null; } }
        public override string AppType { get { return "Source Mod"; } }
        public override string ModIdString { get { return ModFolder; } }

        public SteamLaunchableModSource(uint HostAppID, string ModDir, string ModTitle)
        {
            this.AppID = HostAppID;
            this.ModFolder = Path.GetFileName(ModDir);
            this.ModDir = ModDir;
            this.ModTitle = ModTitle;
        }

        public static SteamLaunchableModSource Make(UInt32 HostAppID, string ModDir, VObject gameInfo = null)
        {
            VObject GameInfoObj = (VObject)(gameInfo.ContainsKey("GameInfo") ? gameInfo["GameInfo"] : null);
            VToken GameObj = GameInfoObj != null ? GameInfoObj.ContainsKey("game") ? GameInfoObj["game"] : null : null;
            VToken IconObj = GameInfoObj != null ? GameInfoObj.ContainsKey("icon") ? GameInfoObj["icon"] : null : null;

            SteamLaunchableModSource src = new SteamLaunchableModSource(HostAppID, ModDir, GameObj?.ToString())
            {
                ModIcon = IconObj?.ToString()
            };

            return src;
        }
    }

    public class SteamLaunchableApp : SteamLaunchable
    {
        public override string Title { get { return Name; } }
        public override string Icon { get { return appIcon; } }
        public override string AppType { get { return appType; } }
        public override string ModIdString { get { return string.Empty; } }

        public override SteamLaunchableType ShortcutType { get { return SteamLaunchableType.App; } }

        public string Name { get; set; }
        public string appIcon { get; set; }
        public string appType { get; set; }
        public string[] extra_developer { get; internal set; }
        public string[] extra_publisher { get; internal set; }
        public bool extra_has_library_capsule { get; internal set; }
        public bool extra_has_library_hero { get; internal set; }
        public bool extra_has_library_logo { get; internal set; }
        public DateTime? extra_original_release_date { get; internal set; }
        public DateTime? extra_steam_release_date { get; internal set; }

        // hopefully the base class will use this because the base is virtual rather than not
        public override UInt32 GenerateModID()
        {
            //return 0 | 0x80000000;
            return 0;
        }

        public SteamLaunchableApp(uint AppID)
        {
            this.AppID = AppID;
        }

        public static SteamLaunchableApp Make(UInt32 AppID)
        {
            SteamLaunchableApp src = new SteamLaunchableApp(AppID)
            {

            };

            return src;
        }
    }

    public class SteamLaunchableShortcut : SteamLaunchable
    {
        public override string Title { get { return "Shortcut"; } }
        public override string Icon { get { return icon; } }
        public override string AppType { get { return "Shortcut"; } }

        public override SteamLaunchableType ShortcutType { get { return SteamLaunchableType.Shortcut; } }

        //"\""
        private string _exe;
        private string _StartDir;
        private string _icon;

        // Minimal for generating ID
        public string appname { get; set; }
        public string exe { get { return "\"" + _exe.Trim('"') + "\""; } set { _exe = value; } }


        public string StartDir { get { return "\"" + _StartDir.Trim('"') + "\""; } set { _StartDir = value; } }
        public string icon { get { return string.IsNullOrEmpty(_icon) ? null : "\"" + _icon.Trim('"') + "\""; } set { _icon = value; } }
        public string ShortcutPath { get; set; }
        public bool hidden { get; set; }
        public bool AllowDesktopConfig { get; set; }
        public bool OpenVR { get; set; }
        public List<string> tags { get; private set; }

        public override string ModIdString { get { return exe + appname; } }

        public SteamLaunchableShortcut(string appname, string exe, string StartDir, string icon = null, string ShortcutPath = null, bool hidden = false, bool AllowDesktopConfig = true, bool OpenVR = false, List<string> tags = null)
        {
            this.appname = appname;
            this.exe = exe;
            this.StartDir = StartDir;
            this.icon = icon;
            this.ShortcutPath = ShortcutPath;
            this.hidden = hidden;
            this.AllowDesktopConfig = AllowDesktopConfig;
            this.OpenVR = OpenVR;
            this.tags = tags ?? new List<string>();
        }

        public SteamLaunchableShortcut(string appname, string exe)
        {
            this.appname = appname;
            this.exe = exe;
        }

        public static SteamLaunchableShortcut Make(string exe, string appname)
        {
            return new SteamLaunchableShortcut(appname, exe);
        }
    }
}
