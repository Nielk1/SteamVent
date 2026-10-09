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
            => SteamApps.GetOwnedApps();

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
        internal void EnsureSteamworksStarted()
        {
            if (!Options.UseSteamworks)
                throw new SteamException("The Steamworks subsystem is disabled (Options.UseSteamworks is false).");
            if (!Client.EnsureSteamworks())
                throw new SteamException("Failed to start Steamworks (is Steam running and logged on?)");
            if (Options.SteamworksAppId is { } forcedId && forcedId > 0 && Client.AppId != forcedId)
                throw new SteamException($"Steamworks is running under AppID {Client.AppId}, but {forcedId} was forced via Options.SteamworksAppId; another context interfered.");
        }

        /// <summary>
        /// Non-throwing readiness check for the in-process Steamworks subsystem: true when it is enabled and has
        /// started successfully. Lets a caller (e.g. an app query) prefer the in-process answer and fall back to the
        /// filesystem instead of throwing when Steamworks is unavailable.
        /// </summary>
        internal bool TryEnsureSteamworks()
        {
            if (!Options.UseSteamworks) return false;
            try { return Client.EnsureSteamworks(); }
            catch { return false; }
        }

        /// <summary>
        /// The in-process <see cref="Steamworks.SteamClient"/> as a nullable - the raw client this context owns,
        /// or null when the Steamworks subsystem is disabled or unavailable. This is what the child facades use to
        /// reach the live client without forcing it to start.
        /// </summary>
        internal Steamworks.SteamClient? ClientIfEnabled
        {
            get
            {
                if (!Options.UseSteamworks) return null;
                try { return Client; } catch { return null; }
            }
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
        internal IWorkshopCommands? TryBridgeCommands()
            => CreateModule<IWorkshopCommands>("SteamVent.Bridge", "SteamVent.Bridge.BridgeSource");

        /// <summary>Bridge adapter (optional assembly) as a workshop source, or null when excised.</summary>
        internal IWorkshopSource? TryBridgeWorkshop()
            => CreateModule<IWorkshopSource>("SteamVent.Bridge", "SteamVent.Bridge.BridgeSource");

        /// <summary>SteamCmd adapter (optional assembly) as a workshop source, or null when excised.</summary>
        internal IWorkshopSource? TrySteamCmdWorkshop()
            => CreateModule<IWorkshopSource>("SteamVent.SteamCmd", "SteamVent.SteamCmd.SteamCmdSource");

        /// <summary>
        /// Workshop items for <paramref name="appid"/>, in a deliberate per-call order: in-process Steamworks
        /// (when we are actually running under that app), else the Bridge, else SteamCmd, else the local
        /// filesystem. Each step is option-gated and degrades to the next when the subsystem is disabled or its
        /// assembly is excised.
        /// </summary>
        public Task<IReadOnlyCollection<PublishedFileData>> GetWorkshopItems(UInt32 appid, CancellationToken cancellationToken = default)
            => SteamWorkshop.GetWorkshopItems(appid, cancellationToken);

        /// <summary>
        /// Owned apps, streaming. Stage 1 (synchronous) enumerates the candidate ids from the filesystem
        /// appcache; stage 2 checks ownership of each candidate against the in-process Steamworks ISteamApps,
        /// bounded by a small concurrency gate so the spot-checks can overlap. Yields one app at a time as they
        /// resolve, so a UI can fill in over time instead of waiting for the whole set.
        /// </summary>
        public IAsyncEnumerable<SteamLaunchableApp> GetOwnedAppsAsync(CancellationToken cancellationToken = default)
            => SteamApps.GetOwnedAppsAsync(cancellationToken);

        #endregion Configurable sources

        public List<SteamLaunchableApp> GetAppsWithMetadata(UInt64[]? AppIDs = null)
            => SteamApps.GetAppsWithMetadata(AppIDs);

        public static readonly string[] GoldSrcModSkipFolders = SteamVent.SteamApps.GoldSrcModSkipFolders;
        public List<SteamLaunchableModGoldSrc> GetGoldSrcMods()
            => SteamApps.GetGoldSrcMods();

        public List<SteamLaunchableModSource> GetSourceMods()
            => SteamApps.GetSourceMods();



        public bool IsInstalled(UInt64 GameID)
            => SteamApps.IsInstalled(GameID);

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
            => SteamApps.GetAppInstallDir(appID);

        public string? GetAppLibrary(UInt32 appId)
            => SteamApps.GetAppLibrary(appId);

        public Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(UInt32 AppId, IProgress<double?>? Progress = null, string LibraryPathOverride = null)
            => SteamWorkshop.WorkshopStatusAsync(AppId, Progress, LibraryPathOverride);




        #region Workshop routing (in-process interproc vs Bridge)

        /// <summary>
        /// The workshop command provider to use for <paramref name="appid"/>, in a deliberate order: in-process
        /// Steamworks when we are actually running under that app (fast, no child process), else the Bridge
        /// (default-off, optional assembly). SteamCmd does not provide appid-context commands, so it is not part
        /// of this path. Returns null when no provider applies, so callers can report the command as unavailable.
        /// </summary>
        public IWorkshopCommands? GetWorkshopCommands(UInt32 appid)
            => SteamWorkshop.GetWorkshopCommands(appid);

        /// <summary>Subscribes to a workshop item, routed in-process or via the Bridge for <paramref name="appid"/>.</summary>
        public Task<RemoteStorageSubscribePublishedFileResult_t> WorkshopSubscribeAsync(ulong publishedFileId, UInt32 appid, CancellationToken cancellationToken = default)
            => SteamWorkshop.WorkshopSubscribeAsync(publishedFileId, appid, cancellationToken);

        /// <summary>Unsubscribes from a workshop item, routed in-process or via the Bridge for <paramref name="appid"/>.</summary>
        public Task<RemoteStorageUnsubscribePublishedFileResult_t> WorkshopUnsubscribeAsync(ulong publishedFileId, UInt32 appid, CancellationToken cancellationToken = default)
            => SteamWorkshop.WorkshopUnsubscribeAsync(publishedFileId, appid, cancellationToken);

        /// <summary>
        /// Downloads a workshop item, routed in-process or via the Bridge for <paramref name="appid"/>, completing
        /// only when it is fully installed. Reports <paramref name="progress"/> samples while it transfers.
        /// </summary>
        public Task<DownloadItemResult_t> WorkshopDownloadAsync(ulong publishedFileId, UInt32 appid, IProgress<WorkshopItemProgress>? progress = null, CancellationToken cancellationToken = default)
            => SteamWorkshop.WorkshopDownloadAsync(publishedFileId, appid, progress, cancellationToken);

        #endregion Workshop routing
    }
}
