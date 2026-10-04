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
using SteamVent.Core.BVdf;
using SteamVent.Steamworks.Native.Interfaces;
using SteamVent.Steamworks.Native;
using Gameloop.Vdf;
using SteamVent.Core;
using SteamVent.Core.Steamworks;
using System.Reflection;

namespace SteamVent
{
    public class SteamException : Exception
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



        // Steamworks lifecycle is owned by the shared SteamClient wrapper (see the Client facade below);
        // constructing the context never requires Steam to be running.
        private SteamContext()
        {
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
                lock (_childLock)
                {
                    _appsBox = null;
                    _workshopBox = null;
                }
                _client?.Dispose();
                _client = null;
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


        #region Child services (SteamVent facades over the Steamworks child implementation)

        private Steamworks.SteamClient? _client;
        private readonly object _childLock = new object();
        private SteamApps? _appsBox;
        private SteamWorkshop? _workshopBox;

        /// <summary>
        /// The shared Steamworks lifecycle wrapper (a ref-counted singleton), owned by the context. Lazily
        /// constructed and never requires Steam to be running. This is an internal building block, not part of
        /// the public surface - the facades below are how you reach it. If you want the raw client directly, use
        /// <c>Steamworks.SteamClient</c> yourself.
        /// </summary>
        private Steamworks.SteamClient Client
        {
            get
            {
                if (!Options.UseSteamworks)
                    throw new SteamException("The Steamworks subsystem is disabled (Options.UseSteamworks is false).");
                if (_client == null)
                {
                    var client = Steamworks.SteamClient.Construct();
                    // Request the configured appid context before the Steamworks context starts, so the
                    // in-process context binds to that appid (via the SteamAppId env var). After it starts the
                    // effective AppID is whatever ISteamUtils.GetAppID reports (see SteamClient.AppId).
                    if (Options.SteamworksAppId.HasValue)
                    {
                        try { client.AppId = Options.SteamworksAppId.Value; }
                        catch (InvalidOperationException) { /* context already active - keep the live value */ }
                    }
                    _client = client;
                }
                return _client;
            }
        }

        /// <summary>The in-process <see cref="Steamworks.SteamApps"/> child (lazily owned by the client).</summary>
        internal Steamworks.SteamApps InnerApps => Client.GetSteamApps();

        /// <summary>The in-process <see cref="Steamworks.SteamWorkshop"/> child (lazily owned by the client).</summary>
        internal Steamworks.SteamWorkshop InnerWorkshop => Client.GetSteamWorkshop();

        /// <summary>
        /// The SteamVent <b>app</b> facade (a latched box over the in-process Steamworks app queries). The context
        /// maintains the underlying client and hands the box the <see cref="Steamworks.SteamApps"/> it needs;
        /// callers only see this facade and its answers come from the configured sources.
        /// </summary>
        public SteamApps SteamApps
        {
            get { lock (_childLock) { return _appsBox ??= new SteamApps(this); } }
        }

        /// <summary>
        /// The SteamVent <b>workshop</b> facade (a latched box over workshop reads + commands). The context routes
        /// each call in-process, via the Bridge, or via SteamCmd according to <see cref="Options"/>; callers only
        /// see this facade and never which module answered.
        /// </summary>
        public SteamWorkshop SteamWorkshop
        {
            get { lock (_childLock) { return _workshopBox ??= new SteamWorkshop(this); } }
        }

        /// <summary>
        /// True when in-process (Steamworks) workshop is the eligible path for <paramref name="appid"/> - i.e.
        /// the Steamworks source is enabled and the local context is actually running under that appid (the
        /// effective AppID from <see cref="Steamworks.ISteamUtils.GetAppID"/>, not merely the configured one).
        /// </summary>
        internal bool UsesInProcessWorkshop(UInt32 appid)
            => Options.UseSteamworks && Client.AppId == appid;

        /// <summary>
        /// The AppID the local (in-process) Steamworks context is actually running under, as reported by
        /// <see cref="Steamworks.ISteamUtils.GetAppID"/> once the context is active; null before it starts or
        /// when Steamworks is unavailable. This is the source of truth used for in-process workshop routing.
        /// </summary>
        public UInt32? ActiveSteamworksAppId
            => Options.UseSteamworks ? Client.AppId : null;

        #endregion Child services


        // TODO use a better AppID source, the AppInfo.vtf file only holds data the client has actually seen
        // Requires: Steam is installed and has AppCache data
        // Requires: Steam is running for SteamApps interface
        public List<SteamLaunchableApp>? GetOwnedApps()
        {
            // Ownership requires the in-process Steamworks ISteamApps check; the candidate list comes from the
            // filesystem appcache. Both are deliberately required for this call.
            EnsureSteamworksStarted();

            Steamworks.SteamApps steamApps = InnerApps;
            List<SteamLaunchableApp> apps = new List<SteamLaunchableApp>();

            IReadOnlyList<SteamAppInfo> candidates;
            try { candidates = SteamAppInfoDataFile.GetApps(); }
            catch { return apps; } // no appcache available -> no candidates (degrade, don't throw)

            foreach (SteamAppInfo app in candidates)
            {
                if (string.IsNullOrWhiteSpace(app.Type) || app.Type == "config")
                    continue;
                if (steamApps.IsSubscribed(app.AppId) != true || string.IsNullOrWhiteSpace(app.Name))
                    continue;

                apps.Add(new SteamLaunchableApp(app.AppId)
                {
                    Name = app.Name!.TrimEnd(),
                    appIcon = app.IconPath,
                    appType = app.Type,
                });
            }

            return apps;
        }

        #region Configurable sources (built-in + optional Bridge/SteamCmd modules)

        private SteamVentOptions _options = SteamVentOptions.Default;

        /// <summary>
        /// Options driving which subsystems each top-level function may use. Set before first use; a disabled
        /// subsystem is never used (and, for Steamworks, never instantiated).
        /// </summary>
        public SteamVentOptions Options
        {
            get => _options;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                lock (_childLock)
                {
                    _options = value;
                    // New options take effect immediately for the lazily-built facades. (A started Steamworks
                    // context cannot rebind its AppID, so set SteamworksAppId before first use.)
                    _appsBox = null;
                    _workshopBox = null;
                }
            }
        }

        /// <summary>
        /// Gate for calls that fundamentally require the in-process Steamworks subsystem. Throws a clear
        /// <see cref="SteamException"/> when it is disabled or cannot start, and - when an AppID was forced via
        /// <see cref="SteamVentOptions.SteamworksAppId"/> - when the live context is not actually running under
        /// it (something else got in the way).
        /// </summary>
        private void EnsureSteamworksStarted()
        {
            if (!Options.UseSteamworks)
                throw new SteamException("The Steamworks subsystem is disabled (Options.UseSteamworks is false).");
            if (!Client.EnsureSteamworks())
                throw new SteamException("Failed to start Steamworks (is Steam running and logged on?)");
            if (Options.SteamworksAppId is { } forcedId && forcedId > 0 && Client.AppId != forcedId)
                throw new SteamException($"Steamworks is running under AppID {Client.AppId}, but {forcedId} was forced via Options.SteamworksAppId; another context interfered.");
        }

        /// <summary>
        /// Deliberately create a specific, known child module by its assembly and type name (NOT auto-discovery:
        /// the caller names exactly which module it wants). Returns null when the assembly is absent (excised)
        /// or the type cannot be constructed, so a missing optional module degrades the calling function instead
        /// of breaking it.
        /// </summary>
        private static T? CreateModule<T>(string assemblyName, string typeName) where T : class
        {
            try
            {
                Assembly? assembly = Assembly.Load(new AssemblyName(assemblyName));
                Type? type = assembly?.GetType(typeName, throwOnError: false);
                ConstructorInfo? ctor = type?.GetConstructor(Type.EmptyTypes);
                return ctor?.Invoke(null) as T;
            }
            catch
            {
                return null; // excised / failed to load -> degrade, don't throw
            }
        }

        /// <summary>Bridge adapter (optional assembly) as an appid-context command provider, or null when excised.</summary>
        private IWorkshopCommands? TryBridgeCommands()
            => CreateModule<IWorkshopCommands>("SteamVent.Bridge", "SteamVent.Bridge.BridgeSource");

        /// <summary>Bridge adapter (optional assembly) as a workshop source, or null when excised.</summary>
        private IWorkshopSource? TryBridgeWorkshop()
            => CreateModule<IWorkshopSource>("SteamVent.Bridge", "SteamVent.Bridge.BridgeSource");

        /// <summary>SteamCmd adapter (optional assembly) as a workshop source, or null when excised.</summary>
        private IWorkshopSource? TrySteamCmdWorkshop()
            => CreateModule<IWorkshopSource>("SteamVent.SteamCmd", "SteamVent.SteamCmd.SteamCmdSource");

        /// <summary>
        /// Workshop items for <paramref name="appid"/>, in a deliberate per-call order: in-process Steamworks
        /// (when we are actually running under that app), else the Bridge, else SteamCmd, else the local
        /// filesystem. Each step is option-gated and degrades to the next when the subsystem is disabled or its
        /// assembly is excised.
        /// </summary>
        public Task<IReadOnlyCollection<PublishedFileData>> GetWorkshopItems(UInt32 appid, CancellationToken cancellationToken = default)
        {
            int appId = (int)appid;

            if (Options.UseSteamworks && UsesInProcessWorkshop(appid))
                return InnerWorkshop.GetInstalledWorkshopItems(appId, cancellationToken);

            if (Options.UseBridge && TryBridgeWorkshop() is IWorkshopSource bridge)
                return bridge.GetInstalledWorkshopItems(appId, cancellationToken);

            if (Options.UseSteamCmd && TrySteamCmdWorkshop() is IWorkshopSource cmd)
                return cmd.GetInstalledWorkshopItems(appId, cancellationToken);

            // Final fallback: whatever is physically on disk under this app's workshop content dir.
            return Task.FromResult(GetWorkshopItemsFromDisk(appid));
        }

        /// <summary>Local workshop items physically present under <c>&lt;library&gt;/steamapps/workshop/content/{appid}</c>.</summary>
        private static IReadOnlyCollection<PublishedFileData> GetWorkshopItemsFromDisk(UInt32 appid)
        {
            var result = new List<PublishedFileData>();
            var seen = new HashSet<ulong>();
            foreach (var library in SteamProcessInfo.GetSteamLibraryPaths())
            {
                if (string.IsNullOrEmpty(library)) continue;
                string contentDir = Path.Combine(library, "steamapps", "workshop", "content", appid.ToString());
                if (!Directory.Exists(contentDir)) continue;
                foreach (var dir in Directory.EnumerateDirectories(contentDir))
                {
                    if (!ulong.TryParse(Path.GetFileName(dir), out var id)) continue;
                    if (!seen.Add(id)) continue;
                    result.Add(new PublishedFileData(id, EItemState.k_EItemStateInstalled) { Folder = dir });
                }
            }
            return result;
        }

        /// <summary>
        /// Owned apps, streaming. Stage 1 (synchronous) enumerates the candidate ids from the filesystem
        /// appcache; stage 2 checks ownership of each candidate against the in-process Steamworks ISteamApps,
        /// bounded by a small concurrency gate so the spot-checks can overlap. Yields one app at a time as they
        /// resolve, so a UI can fill in over time instead of waiting for the whole set.
        /// </summary>
        public async IAsyncEnumerable<SteamLaunchableApp> GetOwnedAppsAsync(CancellationToken cancellationToken = default)
        {
            if (!Options.UseFileSystem)
                throw new SteamException("The FileSystem subsystem is disabled (Options.UseFileSystem is false), so candidate apps cannot be enumerated.");
            if (!Options.UseSteamworks)
                throw new SteamException("The Steamworks subsystem is disabled (Options.UseSteamworks is false), so ownership cannot be determined.");

            IReadOnlyList<SteamAppInfo> candidates;
            try { candidates = SteamAppInfoDataFile.GetApps(); }
            catch { yield break; } // no appcache -> no candidates (degrade, don't throw)

            const int concurrency = 4; // bounded parallelism for the ownership spot-checks
            using var gate = new SemaphoreSlim(concurrency);
            foreach (var app in candidates)
            {
                if (string.IsNullOrWhiteSpace(app.Type) || app.Type == "config" || string.IsNullOrWhiteSpace(app.Name))
                    continue;

                // A synchronous Steamworks interop call, run under the gate so the batch of spot-checks overlap.
                bool? owned = null;
                await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    owned = await Task.Run(() => InnerApps.IsSubscribed(app.AppId), cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    gate.Release();
                }

                if (owned == true)
                    yield return new SteamLaunchableApp(app.AppId)
                    {
                        Name = app.Name!.TrimEnd(),
                        appIcon = app.IconPath,
                        appType = app.Type,
                    };
            }
        }

        #endregion Configurable sources

        public List<SteamLaunchableApp> GetAppsWithMetadata(UInt64[]? AppIDs = null)
        {
            EnsureSteamworksStarted();

            List<SteamLaunchableApp> apps = new List<SteamLaunchableApp>();

            SteamAppInfoDataFile dataFile = SteamAppInfoDataFile.GetSteamAppInfoDataFile();

            foreach(var chunk in dataFile.chunks)
            {
                if (chunk.data != null
                 && chunk.data.Properties != null
                 && chunk.data.Properties.Count > 0
                 && ((AppIDs?.Length ?? 0) == 0 || AppIDs.Contains(chunk.AppID)))
                {
                    BVPropertyCollection? appinfo = ((BVPropertyCollection?)chunk.data?["appinfo"]);
                    BVPropertyCollection? common = ((BVPropertyCollection?)appinfo?["common"]);
                    BVPropertyCollection? extended = ((BVPropertyCollection?)appinfo?["extended"]);

                    string? type = common?["type"]?.GetValue<string>()?.ToLowerInvariant();
                    //if (type == "demo" || type == "game")
                    if (!string.IsNullOrWhiteSpace(type) && type != "config")
                    {
                        bool isInstalled = InnerApps.GetAppInstalled(chunk.AppID) == true;
                        bool isSubscribed = InnerApps.IsSubscribed(chunk.AppID) == true;

                        string? name = common?["name"]?.GetValue<string>();
                        //string oslist = common?["oslist"]?.GetValue<string>();
                        //string icon = common?["icon"]?.GetValue<string>();
                        //string clienttga = common?["clienttga"]?.GetValue<string>();
                        string? clienticon = common?["clienticon"]?.GetValue<string>();
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
            }

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

                            string? AppInstallDir = InnerApps.GetAppInstallDir(appIdCheck);
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
                    return InnerApps.GetAppInstalled(gameID.AppID) == true;
                //return SteamApps.BIsAppInstalled(gameID.AppID().m_AppId);

                // Mod Steam App
                case CGameID.EGameID.k_EGameIDTypeGameMod:
                    return true;

                    // If the base game isn't installed, just say no
                    if (InnerApps.GetAppInstalled(gameID.AppID) != true) return false;
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

        public string? GetAppInstallDir(UInt32 appID)
        {
            // Prefer the in-process Steamworks answer when that subsystem is enabled and running.
            if (Options.UseSteamworks && Client.EnsureSteamworks() && InnerApps.GetAppInstalled(appID) == true)
            {
                string? dir = InnerApps.GetAppInstallDir(appID);
                if (!string.IsNullOrWhiteSpace(dir))
                    return dir;
            }

            // Otherwise fall back to the filesystem (appmanifest), which serves as guidance for locating the app.
            return FileSystem.SteamApps.GetAppInstallDir(appID);
        }

        public string? GetAppLibrary(UInt32 appId)
        {
            string? InstallDir = GetAppInstallDir(appId);
            if (InstallDir != null)
            {
                var Paths = SteamProcessInfo.GetSteamLibraryPaths();
                foreach (string path in Paths)
                    if (!Path.GetRelativePath(path, InstallDir).Contains(".."))
                        return path;
            }
            return null;
        }

        public async Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(UInt32 AppId, IProgress<double?>? Progress = null, string LibraryPathOverride = null)
        {
            string? LibraryPath = LibraryPathOverride ?? GetAppLibrary(AppId);
            if (LibraryPath == null)
                return null;

            return await FileSystem.SteamWorkshop.WorkshopStatusAsync(LibraryPath, AppId, Progress);
        }




        #region Workshop routing (in-process interproc vs Bridge)

        /// <summary>
        /// The workshop command provider to use for <paramref name="appid"/>, in a deliberate order: in-process
        /// Steamworks when we are actually running under that app (fast, no child process), else the Bridge
        /// (default-off, optional assembly). SteamCmd does not provide appid-context commands, so it is not part
        /// of this path. Returns null when no provider applies, so callers can report the command as unavailable.
        /// </summary>
        public IWorkshopCommands? GetWorkshopCommands(UInt32 appid)
        {
            if (Options.UseSteamworks && UsesInProcessWorkshop(appid))
                return InnerWorkshop;

            if (Options.UseBridge)
                return TryBridgeCommands();

            return null;
        }

        private IWorkshopCommands RequireWorkshopCommands(UInt32 appid)
            => GetWorkshopCommands(appid)
               ?? throw new SteamException($"No workshop command provider is available for appid {appid}. " +
                                           "Enable the Bridge, or enable Steamworks and set Options.SteamworksAppId to this appid.");

        /// <summary>Subscribes to a workshop item, routed in-process or via the Bridge for <paramref name="appid"/>.</summary>
        public Task<RemoteStorageSubscribePublishedFileResult_t> WorkshopSubscribeAsync(ulong publishedFileId, UInt32 appid, CancellationToken cancellationToken = default)
            => RequireWorkshopCommands(appid).SubscribeAsync(publishedFileId, (int)appid, cancellationToken);

        /// <summary>Unsubscribes from a workshop item, routed in-process or via the Bridge for <paramref name="appid"/>.</summary>
        public Task<RemoteStorageUnsubscribePublishedFileResult_t> WorkshopUnsubscribeAsync(ulong publishedFileId, UInt32 appid, CancellationToken cancellationToken = default)
            => RequireWorkshopCommands(appid).UnsubscribeAsync(publishedFileId, (int)appid, cancellationToken);

        /// <summary>Downloads a workshop item, routed in-process or via the Bridge for <paramref name="appid"/>.</summary>
        public Task<DownloadItemResult_t> WorkshopDownloadAsync(ulong publishedFileId, UInt32 appid, CancellationToken cancellationToken = default)
            => RequireWorkshopCommands(appid).DownloadAsync(publishedFileId, (int)appid, cancellationToken);

        #endregion Workshop routing
    }
}
