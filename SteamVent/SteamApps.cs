using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using SteamVent.Core;
using SteamVent.Core.BVdf;
using SteamVent.FileSystem;

namespace SteamVent
{
    /// <summary>
    /// The SteamVent <b>app</b> facade - owns the app-level queries (owned apps, install state, install directory,
    /// app metadata, engine mods). Obtain it from <see cref="SteamContext.SteamApps"/>. Methods that require the
    /// in-process Steamworks subsystem throw a clear <see cref="SteamException"/> when it is disabled; the
    /// filesystem-backed queries (install directory, mods) degrade gracefully. The options and in-process client
    /// are supplied by the <see cref="SteamContext"/> this facade was boxed over.
    /// </summary>
    public sealed class SteamApps
    {
        private readonly SteamContext _context;

        internal SteamApps(SteamContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private Steamworks.SteamApps? Inner
        {
            get
            {
                if (!_context.Options.UseSteamworks) return null;
                try { return _context.InnerApps; } catch { return null; }
            }
        }

        /// <summary>True when the in-process app interface is reachable right now (Steam running and logged on).</summary>
        public bool IsAvailable
        {
            get
            {
                var inner = Inner;
                if (inner == null) return false;
                try { return inner.GetAppInstalled(0) != null; } catch { return false; }
            }
        }

        /// <summary>Install directory for the app (in-process when available, else the filesystem), or null when it cannot be located.</summary>
        public string? GetAppInstallDir(UInt32 appId)
        {
            // Prefer the in-process Steamworks answer when that subsystem is enabled and running.
            if (_context.Options.UseSteamworks && _context.TryEnsureSteamworks() && _context.InnerApps.GetAppInstalled(appId) == true)
            {
                string? dir = _context.InnerApps.GetAppInstallDir(appId);
                if (!string.IsNullOrWhiteSpace(dir))
                    return dir;
            }

            // Otherwise fall back to the filesystem (appmanifest), which serves as guidance for locating the app.
            return FileSystem.SteamApps.GetAppInstallDir(appId);
        }

        /// <summary>Whether the app is installed; null when Steamworks is unavailable.</summary>
        public bool? GetAppInstalled(uint appId) => Inner?.GetAppInstalled(appId);

        /// <summary>Whether the current user owns (is subscribed to) the app; null when Steamworks is unavailable.</summary>
        public bool? IsSubscribed(uint appId) => Inner?.IsSubscribed(appId);

        #region Owned apps (moved from SteamContext)

        /// <summary>
        /// Owned apps (synchronous). Ownership requires the in-process Steamworks ISteamApps check; the candidate
        /// list comes from the filesystem appcache. Both are deliberately required for this call.
        /// </summary>
        public List<SteamLaunchableApp>? GetOwnedApps()
        {
            _context.EnsureSteamworksStarted();

            Steamworks.SteamApps steamApps = _context.InnerApps;
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

        /// <summary>
        /// Owned apps, streaming. Stage 1 (synchronous) enumerates the candidate ids from the filesystem
        /// appcache; stage 2 checks ownership of each candidate against the in-process Steamworks ISteamApps,
        /// bounded by a small concurrency gate so the spot-checks can overlap. Yields one app at a time as they
        /// resolve, so a UI can fill in over time instead of waiting for the whole set.
        /// </summary>
        public async IAsyncEnumerable<SteamLaunchableApp> GetOwnedAppsAsync(CancellationToken cancellationToken = default)
        {
            if (!_context.Options.UseFileSystem)
                throw new SteamException("The FileSystem subsystem is disabled (Options.UseFileSystem is false), so candidate apps cannot be enumerated.");
            if (!_context.Options.UseSteamworks)
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
                    owned = await Task.Run(() => _context.InnerApps.IsSubscribed(app.AppId), cancellationToken).ConfigureAwait(false);
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

        #endregion Owned apps

        #region App metadata (moved from SteamContext)

        public List<SteamLaunchableApp> GetAppsWithMetadata(UInt64[]? AppIDs = null)
        {
            _context.EnsureSteamworksStarted();

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
                    if (!string.IsNullOrWhiteSpace(type) && type != "config")
                    {
                        bool isInstalled = _context.InnerApps.GetAppInstalled(chunk.AppID) == true;
                        bool isSubscribed = _context.InnerApps.IsSubscribed(chunk.AppID) == true;

                        string? name = common?["name"]?.GetValue<string>();
                        string? clienticon = common?["clienticon"]?.GetValue<string>();

                        List<string> developer = new List<string>();
                        List<string> publisher = new List<string>();

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
                                    if (elemType == "developer")
                                    {
                                        developer.Add(elemVal);
                                    }
                                    if (elemType == "publisher")
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

        #endregion App metadata

        #region Engine mods + app location (moved from SteamContext)

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

                            string? AppInstallDir = _context.InnerApps.GetAppInstallDir(appIdCheck);
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
                    return _context.InnerApps.GetAppInstalled(gameID.AppID) == true;
                // Mod Steam App
                case CGameID.EGameID.k_EGameIDTypeGameMod:
                    return true;

                    // If the base game isn't installed, just say no
                    if (_context.InnerApps.GetAppInstalled(gameID.AppID) != true) return false;

                    // Root app is GoldSrc
                    if(gameID.AppID == 70)
                    {
                        List<SteamLaunchableModGoldSrc> mods = GetGoldSrcMods();
                        return mods.Any(dr => dr.GetShortcutID() == GameID);
                    }

                    // Root app is Source
                    {
                        List<SteamLaunchableModSource> mods = GetSourceMods();
                        return mods.Any(dr => dr.GetShortcutID() == GameID);
                    }

                    return false;
                case CGameID.EGameID.k_EGameIDTypeShortcut:
                    break;
            }

            return false;
        }

        /// <summary>The Steam library path that contains the app's install directory, or null when it cannot be located.</summary>
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

        #endregion Engine mods + app location
    }
}