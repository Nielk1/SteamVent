using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core.Steamworks;

namespace SteamVent.Core
{
    /// <summary>
    /// The set of optional sources the top-level context can compose. Treats the whole library as a
    /// pipeline of interchangeable providers: FileSystem/Steamworks are built in; Bridge/SteamCmd are
    /// optional modules that degrade out of the picture entirely when their assemblies are absent.
    /// </summary>
    [Flags]
    public enum SteamSource
    {
        None = 0,
        FileSystem = 1 << 0,
        Steamworks = 1 << 1,
        Bridge = 1 << 2,
        SteamCmd = 1 << 3,
        All = None | FileSystem | Steamworks | Bridge | SteamCmd
    }

    /// <summary>A single app record discovered from a source (a candidate, or a confirmed owned app).</summary>
    public sealed record OwnedAppInfo(uint AppId, string Name, string Type, string Icon);

    /// <summary>
    /// The base identity of any SteamVent source. Built-ins (FileSystem, Steamworks) and optional
    /// modules (Bridge, SteamCmd) all implement this; the top-level context composes whatever is
    /// present and degrades gracefully when a source is missing or unavailable.
    /// </summary>
    public interface ISource
    {
        string Name { get; }
        /// <summary>Lower is preferred. Ties are resolved deterministically by name.</summary>
        int Priority { get; }

        /// <summary>Which optional subsystem this source belongs to; used to honour SteamVentOptions.Sources.</summary>
        SteamVent.Core.SteamSource Family { get; }

        /// <summary>Fast, side-effect-free availability probe (e.g. "is the assembly loaded").</summary>
        bool IsAvailable();

        /// <summary>Deeper availability check (may spawn a process / open a connection). False ⇒ the source is skipped.</summary>
        Task<bool> CheckAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Workshop <i>status</i> (installed / subscribed) for one appid, from a single source.</summary>
    public interface IWorkshopSource
    {
        Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default);
    }

    /// <summary>App enumeration and ownership for a source (e.g. the FileSystem appcache, or Steamworks).</summary>
    public interface IAppSource
    {
        /// <summary>All app ids this source can see. The union across sources is the candidate set.</summary>
        Task<IReadOnlyCollection<OwnedAppInfo>> GetCandidateApps(CancellationToken cancellationToken = default);

        /// <summary>True if owned, false if known-not-owned, null if this source cannot determine it.</summary>
        Task<bool?> IsOwned(uint appId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Workshop <i>commands</i> that must run inside a specific app's context - subscribe / download /
    /// unsubscribe. This is what the Bridge provides ("Steamworks, but run in an AppID context");
    /// SteamCmd provides it over its pipe. Sources without an app context do not implement this, and
    /// the context simply reports the command as unavailable rather than failing.
    /// </summary>
    public interface IWorkshopCommands
    {
        Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default);
        Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default);
        Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default);
    }

    /// <summary>Configuration that drives which sources the top-level context composes and how.</summary>
    public sealed class SteamVentOptions
    {
        /// <summary>Which source families are enabled. Built-ins honour this; optional modules only load when flagged.</summary>
        public SteamSource Sources { get; init; } = SteamSource.FileSystem | SteamSource.Steamworks;

        /// <summary>
        /// The appid the local (in-process) Steamworks / interproc context is running under - i.e. the app
        /// that owns the calling process (<c>steam_appid.txt</c> / <c>SteamAppId</c>). When set, workshop
        /// commands for that exact appid are served in-process via <c>ISteamUGC</c> (fast, no child process);
        /// any other appid is routed to the Bridge. When <c>null</c> (the default - a host that is not itself
        /// a Steam game, e.g. a standalone mod manager) the in-process path is never eligible and the Bridge
        /// is always used.
        /// </summary>
        public UInt32? SteamworksAppId { get; init; }

        /// <summary>Directory probed (Assembly.Load by name) for optional source assemblies.</summary>
        public string? ModuleDirectory { get; init; }

        /// <summary>Names of optional source assemblies to try to load; missing names are skipped, never thrown.</summary>
        public IReadOnlyCollection<string> ModuleAssemblyNames { get; init; } =
            new[] { "SteamVent.Bridge", "SteamVent.SteamCmd" };

        /// <summary>Extra preloaded assemblies to scan for ISource implementations (e.g. in unit tests).</summary>
        public IReadOnlyCollection<Assembly> AdditionalAssemblies { get; init; } = Array.Empty<Assembly>();

        /// <summary>Max number of sources executed concurrently for a single query.</summary>
        public int MaxParallelism { get; init; } = 2;

        /// <summary>Per-source operation timeout; slower sources are dropped and the answer degrades.</summary>
        public TimeSpan SourceTimeout { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>Register source instances directly (used by tests and advanced callers); takes precedence over discovery.</summary>
        public IReadOnlyCollection<ISource> SourceInstances { get; init; } = Array.Empty<ISource>();

        public static SteamVentOptions Default { get; } = new SteamVentOptions();
    }
}