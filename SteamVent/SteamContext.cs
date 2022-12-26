using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Gameloop.Vdf.Linq;
using SteamVent.FileSystem;
using SteamVent.Common.BVdf;
using SteamVent.InterProc.Interfaces;
using SteamVent.InterProc;
using Gameloop.Vdf;
using SteamVent.Common;

namespace SteamVent
{
    class SteamException : Exception
    {
        public SteamException(string msg)
            : base(msg)
        {
        }
    }

    /// <summary>
    /// General context wrapper for entire SteamVent system utilizing what sub-libraries are available
    /// </summary>
    public class SteamContext : IDisposable
    {
        private static readonly object CoreInstanceMutex = new object();
        private static SteamContext CoreInstance;



        private ISteamClient SteamClient { get; set; }
        Type _SteamClientVersion { get; set; }
        private Int32 Pipe { get; set; }
        private Int32 User { get; set; }
        private ISteamApps SteamApps { get; set; }
        Type _SteamAppsVersion { get; set; }


        private SteamContext()
        {
            Init();
        }

        public static SteamContext GetInstance()
        {
            lock (CoreInstanceMutex)
            {
                if (CoreInstance == null)
                    CoreInstance = new SteamContext();
                return CoreInstance;
            }
        }

        #region Dispose
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Shutdown();
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        ~SteamContext()
        {
            Dispose(false);
        }
        #endregion Dispose



        public bool SteamIsRunning { get { return SteamProcessInfo.GetSteamPid() > 0; } }


        // TODO use a better AppID source, the AppInfo.vtf file only holds data the client has actually seen
        public List<SteamLaunchableApp> GetOwnedApps()
        {
            //UInt32[] appIDs = GetClientAppIds();
            List<SteamLaunchableApp> apps = new List<SteamLaunchableApp>();

            //SteamAppInfoDataFile dataFile = SteamAppInfoDataFile.Read(GetAppCacheAppInfoFile());
            SteamAppInfoDataFile dataFile = SteamAppInfoDataFile.GetSteamAppInfoDataFile();

            /*dataFile.chunks
                .Where(chunk => chunk.data != null && chunk.data.Properties != null && chunk.data.Properties.Count > 0)
                //.Select(chunk => ((BVStringToken)((BVPropertyCollection)((BVPropertyCollection)chunk.data?["appinfo"])?["common"])?["type"])?.Value?.ToLowerInvariant())
                .Select(chunk => ((BVStringToken)((BVPropertyCollection)((BVPropertyCollection)chunk.data?["appinfo"])?["common"])?["releasestate"])?.Value?.ToLowerInvariant())
                .Distinct()
                .OrderBy(dr => dr)
                .ToList()
                .ForEach(dr =>
                {
                    Console.WriteLine(dr);
                });*/

            dataFile.chunks
                .ForEach(chunk =>
                {
                    if (chunk.data != null
                    && chunk.data.Properties != null
                    && chunk.data.Properties.Count > 0)
                    {
                        BVPropertyCollection appinfo = ((BVPropertyCollection)chunk.data?["appinfo"]);
                        BVPropertyCollection common = ((BVPropertyCollection)appinfo?["common"]);
                        BVPropertyCollection extended = ((BVPropertyCollection)appinfo?["extended"]);

                        string type = common?["type"]?.GetValue<string>()?.ToLowerInvariant();
                        //if (type == "demo" || type == "game")
                        if (!string.IsNullOrWhiteSpace(type) && type != "config")
                        {
                            bool isInstalled = SteamApps.BIsAppInstalled(chunk.AppID);
                            bool isSubscribed = SteamApps.BIsSubscribedApp(chunk.AppID);

                            string name = common?["name"]?.GetValue<string>();
                            string clienticon = common?["clienticon"]?.GetValue<string>();

                            if (isSubscribed && !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(type))
                            {
                                apps.Add(new SteamLaunchableApp(chunk.AppID)
                                {
                                    Name = name?.TrimEnd(),
                                    appIcon = string.IsNullOrWhiteSpace(clienticon) ? null : Path.Combine(SteamProcessInfo.SteamInstallPath, "steam", "games", clienticon + ".ico"),
                                    appType = type,
                                });;
                            }
                        }
                    }
                });

            return apps;
        }

        public List<SteamLaunchableApp> GetAppsWithMetadata(UInt64[] AppIDs = null)
        {
            //UInt32[] appIDs = GetClientAppIds();
            List<SteamLaunchableApp> apps = new List<SteamLaunchableApp>();

            //SteamAppInfoDataFile dataFile = SteamAppInfoDataFile.Read(GetAppCacheAppInfoFile());
            SteamAppInfoDataFile dataFile = SteamAppInfoDataFile.GetSteamAppInfoDataFile();

            /*dataFile.chunks
                .Where(chunk => chunk.data != null && chunk.data.Properties != null && chunk.data.Properties.Count > 0)
                //.Select(chunk => ((BVStringToken)((BVPropertyCollection)((BVPropertyCollection)chunk.data?["appinfo"])?["common"])?["type"])?.Value?.ToLowerInvariant())
                .Select(chunk => ((BVStringToken)((BVPropertyCollection)((BVPropertyCollection)chunk.data?["appinfo"])?["common"])?["releasestate"])?.Value?.ToLowerInvariant())
                .Distinct()
                .OrderBy(dr => dr)
                .ToList()
                .ForEach(dr =>
                {
                    Console.WriteLine(dr);
                });*/

            dataFile.chunks
                .ForEach(chunk =>
                {
                    if (chunk.data != null
                    && chunk.data.Properties != null
                    && chunk.data.Properties.Count > 0
                    && ((AppIDs?.Length ?? 0) == 0 || AppIDs.Contains(chunk.AppID)))
                    {
                        BVPropertyCollection appinfo = ((BVPropertyCollection)chunk.data?["appinfo"]);
                        BVPropertyCollection common = ((BVPropertyCollection)appinfo?["common"]);
                        BVPropertyCollection extended = ((BVPropertyCollection)appinfo?["extended"]);

                        string type = common?["type"]?.GetValue<string>()?.ToLowerInvariant();
                        //if (type == "demo" || type == "game")
                        if (!string.IsNullOrWhiteSpace(type) && type != "config")
                        {
                            bool isInstalled = SteamApps.BIsAppInstalled(chunk.AppID);
                            bool isSubscribed = SteamApps.BIsSubscribedApp(chunk.AppID);

                            string name = common?["name"]?.GetValue<string>();
                            //string oslist = common?["oslist"]?.GetValue<string>();
                            //string icon = common?["icon"]?.GetValue<string>();
                            //string clienttga = common?["clienttga"]?.GetValue<string>();
                            string clienticon = common?["clienticon"]?.GetValue<string>();
                            //string logo = common?["logo"]?.GetValue<string>();
                            //string logo_small = common?["logo_small"]?.GetValue<string>();
                            //string releasestate = common?["releasestate"]?.GetValue<string>();
                            //string linuxclienticon = common?["linuxclienticon"]?.GetValue<string>();
                            //string controller_support = common?["controller_support"]?.GetValue<string>();
                            //string clienticns = common?["clienticns"]?.GetValue<string>();
                            //int metacritic_score = ((BVInt32Token)common?["metacritic_score"])?.Value ?? -1;
                            //string metacritic_name = common?["metacritic_name"]?.GetValue<string>();
                            //BVPropertyCollection small_capsule = ((BVPropertyCollection)common?["small_capsule"]);
                            //BVPropertyCollection header_image = ((BVPropertyCollection)common?["header_image"]);
                            //BVPropertyCollection languages = ((BVPropertyCollection)common?["languages"]);
                            //bool community_visible_stats = common?["community_visible_stats"]?.GetValue<string>() == "1";
                            //bool community_hub_visible = common?["community_hub_visible"]?.GetValue<string>() == "1";
                            //bool workshop_visible = common?["workshop_visible"]?.GetValue<string>() == "1";
                            //bool exfgls = common?["exfgls"]?.GetValue<string>() == "1";
                            //string gamedir = extended?["gamedir"]?.GetValue<string>();
                            List<string> developer = new List<string>();
                            List<string> publisher = new List<string>();
                            //string homepage = extended?["homepage"]?.GetValue<string>();
                            //string gamemanualurl = extended?["gamemanualurl"]?.GetValue<string>();
                            //bool showcdkeyonlaunch = extended?["showcdkeyonlaunch"]?.GetValue<string>() == "1";
                            //bool dlcavailableonstore = extended?["dlcavailableonstore"]?.GetValue<string>() == "1";

                            {
                                List<BVProperty> props = (common?["associations"] as BVPropertyCollection)?.Properties;
                                if (props != null)
                                {
                                    foreach (BVProperty element in props)
                                    {
                                        string elemType = (element.Value as BVPropertyCollection)?["type"].GetValue<string>();
                                        string elemVal = (element.Value as BVPropertyCollection)?["name"].GetValue<string>();
                                        if (string.IsNullOrWhiteSpace(elemVal))
                                            continue;
                                        if (elemType == "developer")// && string.IsNullOrWhiteSpace(developer))
                                        {
                                            developer.Add(elemVal);
                                        }
                                        if (elemType == "publisher")// && string.IsNullOrWhiteSpace(publisher))
                                        {
                                            publisher.Add(elemVal);
                                        }
                                    }
                                }
                                string extended_dev = extended?["developer"]?.GetValue<string>();
                                if (developer.Count == 0 && !string.IsNullOrWhiteSpace(extended_dev))
                                    developer.Add(extended_dev);
                                string extended_pub = extended?["publisher"]?.GetValue<string>();
                                if (publisher.Count == 0 && !string.IsNullOrWhiteSpace(extended_pub))
                                    publisher.Add(extended_pub);
                            }

                            long? _original_release_date = common?["original_release_date"]?.GetValue<long>();
                            long? _steam_release_date = common?["steam_release_date"]?.GetValue<long>();
                            DateTime? original_release_date = _original_release_date.HasValue ? DateTimeOffset.FromUnixTimeSeconds(_original_release_date.Value).UtcDateTime : (DateTime?)null;
                            DateTime? steam_release_date = _steam_release_date.HasValue ? DateTimeOffset.FromUnixTimeSeconds(_steam_release_date.Value).UtcDateTime : (DateTime?)null;

                            BVPropertyCollection library_assets = ((BVPropertyCollection)common?["library_assets"]);
                            bool has_library_capsule = !string.IsNullOrWhiteSpace(library_assets?["library_capsule"]?.GetValue<string>());
                            bool has_library_hero = !string.IsNullOrWhiteSpace(library_assets?["library_hero"]?.GetValue<string>());
                            bool has_library_logo = !string.IsNullOrWhiteSpace(library_assets?["library_logo"]?.GetValue<string>());

                            //Console.WriteLine($"{chunk.AppID}\t{(type ?? string.Empty).PadRight(4)} {(isInstalled ? 1 : 0)} {(isSubscribed ? 1 : 0)} {(releasestate ?? string.Empty).PadRight(11)} {(name ?? string.Empty).PadRight(90)} {(developer ?? string.Empty).PadRight(40)} {(publisher ?? string.Empty)}");
                            //File.AppendAllText("SteamDump.txt",$"{chunk.appID}\t{(type ?? string.Empty).PadRight(4)} {(isInstalled ? 1 : 0)} {(isSubscribed ? 1 : 0)} {(releasestate ?? string.Empty).PadRight(11)} {(name ?? string.Empty).PadRight(90)} {(developer ?? string.Empty).PadRight(40)} {(publisher ?? string.Empty).PadRight(40)}\r\n");

                            if (isSubscribed && !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(type))
                            {
                                apps.Add(new SteamLaunchableApp(chunk.AppID)
                                {
                                    Name = name?.TrimEnd(),
                                    appIcon = string.IsNullOrWhiteSpace(clienticon) ? null : Path.Combine(SteamProcessInfo.SteamInstallPath, "steam", "games", clienticon + ".ico"),
                                    appType = type,

                                    extra_developer = developer.ToArray(),
                                    extra_publisher = publisher.ToArray(),
                                    extra_has_library_capsule = has_library_capsule,
                                    extra_has_library_hero = has_library_hero,
                                    extra_has_library_logo = has_library_logo,

                                    extra_original_release_date = original_release_date,
                                    extra_steam_release_date = steam_release_date,
                                }); ;
                            }
                        }
                    }
                });

            return apps;
        }

        public static readonly string[] GoldSrcModSkipFolders = new string[] {
            "valve", // "valve_hd", // Half-Life
            "gearbox", // "gearbox_hd", // Opposing Force
            "bshift", // "bshift_hd", // Blue Shift
            "dmc", // Deathmatch Classic
            "cstrike", // "cstrike_hd", // Counter-Strike
            "czero", // Condition Zero
            "czeror", // Condition-Zero: Deleted Scenes
            "dod", // Day of Defeat
            "ricochet", // Ricochet
            //"htmlcache", "platform", "Soundtrack", // other
            "before" // mod that even Steam accidently picks up, we're offically better than Steam itself now
        };
        public List<SteamLaunchableModGoldSrc> GetGoldSrcMods()
        {
            // gold source mods
            List<SteamLaunchableModGoldSrc> GoldSourceMods = new List<SteamLaunchableModGoldSrc>();
            string AppInstallDir = SteamProcessInfo.GetGoldSrcModPath();
            UInt32 sourceModAppID = 70;

            if (Directory.Exists(AppInstallDir))
            {
                Directory.GetDirectories(AppInstallDir)
                    .Where(dr => !GoldSrcModSkipFolders.Contains(Path.GetFileName(dr)))
                    .Where(dr => File.Exists(Path.Combine(dr, "liblist.gam")))
                    .ToList().ForEach(dr =>
                    {
                        //VObject rootObj = VdfConvert.Deserialize("\"GameInfo\"\r\n{\r\n" + File.ReadAllText(Path.Combine(dr, "liblist.gam")) + "\r\n}");
                        //VObject tmp = (VObject)rootObj["GameInfo"];
                        //VToken tmp2 = tmp["gamedir"];
                        //UInt64 tmpID = SteamLaunchableShortcut.GetModShortcutID(tmp2.ToString(), sourceModAppID);
                        //if (tmpID > 0) GoldSourceMods.Add(tmpID);

                        SteamLaunchableModGoldSrc mod = SteamLaunchableModGoldSrc.Make(sourceModAppID, dr, Path.Combine(dr, "liblist.gam"));
                        if (mod != null) GoldSourceMods.Add(mod);
                    });
            }
            return GoldSourceMods;
        }

        public List<SteamLaunchableModSource> GetSourceMods()
        {
            // source mods
            List<SteamLaunchableModSource> SourceMods = new List<SteamLaunchableModSource>();
            {
                string sourceMods = SteamProcessInfo.GetSourceModPath();
                if (Directory.Exists(sourceMods))
                {
                    Directory.GetDirectories(sourceMods)
                        .Where(dr => File.Exists(Path.Combine(dr, "gameinfo.txt")))
                        .ToList().ForEach(dr =>
                        {
                            VObject rootObj = new VObject();
                            rootObj.Add(VdfConvert.Deserialize(File.ReadAllText(Path.Combine(dr, "gameinfo.txt"))));
                            VObject GameInfoObj = (VObject)rootObj["GameInfo"];
                            VObject FileSystemObj = (VObject)GameInfoObj["FileSystem"];
                            VToken appID = FileSystemObj["SteamAppId"];

                            UInt32 appIdCheck = 0;
                            if (!UInt32.TryParse(appID.ToString(), out appIdCheck)) return;
                            if (appIdCheck == 0) return;

                            string AppInstallDir = SteamApps.GetAppInstallDir(appIdCheck);
                            if (!string.IsNullOrWhiteSpace(AppInstallDir))
                            {
                                SteamLaunchableModSource mod = SteamLaunchableModSource.Make(appIdCheck, dr, rootObj);
                                if (mod != null) SourceMods.Add(mod);
                            }
                        });
                }
            }
            return SourceMods;
        }



        public bool IsInstalled(UInt64 GameID)
        {
            CGameID gameID = new CGameID(GameID);
            switch (gameID.AppType)
            {
                // Basic Steam App
                case CGameID.EGameID.k_EGameIDTypeApp:
                    return SteamApps.BIsAppInstalled(gameID.AppID);
                //return SteamApps.BIsAppInstalled(gameID.AppID().m_AppId);

                // Mod Steam App
                case CGameID.EGameID.k_EGameIDTypeGameMod:
                    return true; // just assume a mod is installed for now, might need change later
#if false
                    // If the base game isn't installed, just say no
                    if (!SteamApps.BIsAppInstalled(gameID.AppID)) return false;
                    //if (!SteamApps.BIsAppInstalled(gameID.AppID().m_AppId)) return false;

                    // Root app is GoldSrc
                    //if (GoldSrcModHosts.Contains(gameID.AppID().m_AppId))
                    if(gameID.AppID == 70)
                    {
                        // Get a list of known GoldSrc Mods
                        List<SteamLaunchableModGoldSrc> mods = GetGoldSrcMods();

                        // return if any of these mods match our ID
                        return mods.Any(dr => dr.GetShortcutID() == GameID);
                    }

                    // Root app is Source
                    // TODO add check for source engine IDs here
                    {
                        // Get a list of known GoldSrc Mods
                        List<SteamLaunchableModSource> mods = GetSourceMods();

                        // return if any of these mods match our ID
                        return mods.Any(dr => dr.GetShortcutID() == GameID);
                    }

                    return false;
#endif

                case CGameID.EGameID.k_EGameIDTypeShortcut:
                    break;
            }

            return false;
        }

        /*public void InstallGame(UInt64 GameID)
        {
            CGameID gameID = new CGameID(GameID);
            switch (gameID.AppType)
            {
                // Basic Steam App
                case CGameID.EGameID.k_EGameIDTypeApp:
                    {
                        string InstallCommand = $"steam://install/{GameID}";
                        string installPath = Steamworks.GetInstallPath();
                        string steamEXE = Path.Combine(installPath, @"steam.exe");
                        Process.Start(steamEXE, InstallCommand);
                    }
                    break;
                case CGameID.EGameID.k_EGameIDTypeGameMod:
                    break;
                case CGameID.EGameID.k_EGameIDTypeShortcut:
                    break;
            }
        }*/

        /*public string[] GetGameLibraries()
        {
            try
            {
                int CountBaseFolders = ClientAppManager.GetNumInstallBaseFolders();
                string[] BaseFolders = new string[CountBaseFolders];
                for (int x = 0; x < CountBaseFolders; x++)
                {
                    StringBuilder builder = new StringBuilder(1024);
                    ClientAppManager.GetInstallBaseFolder(x, builder);
                    BaseFolders[x] = builder.ToString();
                }
                return BaseFolders;
            }
            catch
            {
                return new string[0];
            }
        }*/

        /*public EAppUpdateError? InstallGame(UInt64 GameID, int GameLibraryIndex)
        {
            CGameID gameID = new CGameID(GameID);
            switch (gameID.AppType)
            {
                // Basic Steam App
                case CGameID.EGameID.k_EGameIDTypeApp:
                    {
                        return ClientAppManager.InstallApp(gameID.AppID, GameLibraryIndex, false);
                    }
                    break;
                case CGameID.EGameID.k_EGameIDTypeGameMod:
                    break;
                case CGameID.EGameID.k_EGameIDTypeShortcut:
                    break;
            }
            return null;
        }*/




        /*public void StartBigPicture()
        {
            string installPath = Steamworks.GetInstallPath();
            string steamEXE = Path.Combine(installPath, @"steam.exe");
            Process.Start(steamEXE, "steam://open/bigpicture");
        }*/

        /*public bool IsInBigPicture()
        {
            return WrappedContext.BigPicturePID > 0;
        }*/

        public void Init(string ProxyServerPath = null, bool SearchSubfolders = false)
        {
            Steam.Load();
            SteamClient = Steam.CreateInterface<ISteamClient017>();
            if (SteamClient == null)
                throw new Exception();
            Pipe = SteamClient.CreateSteamPipe();
            if (Pipe == 0)
                throw new Exception();
            User = SteamClient.ConnectToGlobalUser(Pipe);
            if (User == 0)
                throw new Exception();
            SteamApps = SteamClient.GetISteamApps<ISteamApps008>(User, Pipe);
            if (SteamApps == null)
                throw new Exception();
        }

        public void Shutdown()
        {
            if (SteamClient == null)
                return;

            SteamClient.ReleaseUser(Pipe, User);
            SteamClient.BReleaseSteamPipe(Pipe);
        }
    }
}
