using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core.Steamworks;

namespace SteamVent.Core
{

    /// <summary>Workshop <i>status</i> (installed / subscribed) for one appid, from a single source.</summary>
    public interface IWorkshopSource
    {
        Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default);
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
        /// <summary>
        /// Downloads (installs) a workshop item and completes only once it is fully installed and up to date - not
        /// merely when Steam acknowledges the request. Both backends honor this contract, so the caller sees the
        /// same "started, then done / failed / cancelled" behavior regardless of which module answered.
        /// <para>
        /// Pass a non-null <paramref name="progress"/> to receive live <see cref="WorkshopItemProgress"/> samples
        /// (state + bytes transferred) while the content downloads; the samples are throttled to change. The caller
        /// does not need to track the individual transfer chunks - only that it started and that it finished.
        /// </para>
        /// </summary>
        Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, IProgress<WorkshopItemProgress>? progress = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Configuration for the top-level <c>SteamContext</c>. Each subsystem is an explicit, individually
    /// enable-able module: a disabled one is never used (and, for Steamworks, never instantiated), and a
    /// missing optional assembly (Bridge/SteamCmd) degrades the affected functions instead of breaking.
    /// Assign to <c>SteamContext.Options</c> before first use.
    /// </summary>
    public sealed class SteamVentOptions
    {
        /// <summary>
        /// Use the filesystem subsystem (appcache enumeration, app install dirs, local workshop status).
        /// Mostly guidance: a few operations read the filesystem even when this is off, because it is how
        /// certain paths are located. Defaults to on.
        /// </summary>
        public bool UseFileSystem { get; init; } = true;

        /// <summary>
        /// Use the in-process Steamworks subsystem (ownership, install state, in-process workshop). When off,
        /// the Steamworks client is never started. Defaults to on.
        /// </summary>
        public bool UseSteamworks { get; init; } = true;

        /// <summary>
        /// Use the Bridge module (an out-of-process Steamworks scoped to any appid: richer workshop data +
        /// subscribe / unsubscribe / download commands). Optional assembly; off by default.
        /// </summary>
        public bool UseBridge { get; init; }

        /// <summary>
        /// How long a pooled Bridge child process for an appid stays warm after its last command before it is
        /// reaped. Keep this short: an idle bridge still holds a Steam app-context alive (it can make Steam treat
        /// that app as running), so a long hang time is actively harmful. Applies only to the Bridge module;
        /// null = the <c>SteamVent.Bridge.BridgePool</c> default (30s).
        /// </summary>
        public TimeSpan? BridgeIdleTimeout { get; init; }

        /// <summary>
        /// Use the SteamCmd module (workshop status via the SteamCmd automation pipe). Optional assembly; off
        /// by default.
        /// </summary>
        public bool UseSteamCmd { get; init; }

        /// <summary>
        /// Use the Steam Web API subsystem. Requires an API key and/or proxy to function; off by default.
        /// </summary>
        public bool UseWebApi { get; init; }

        /// <summary>
        /// The appid to <b>force</b> the local (in-process) Steamworks context to before it starts. When set
        /// (non-zero), the context binds to that appid via the <c>SteamAppId</c>/<c>StickySteamAppId</c>
        /// environment variables, in-process workshop commands for that exact appid are served by
        /// <c>ISteamUGC</c> (fast, no child process), and the context <i>validates</i> on start that it is
        /// actually running under it - if the live <c>ISteamUtils.GetAppID</c> differs, an exception is thrown
        /// because something else got in the way. When null/zero (the default - a host that is not itself a
        /// Steam game), the in-process path is only eligible once Steamworks reports a matching appid.
        /// </summary>
        public UInt32? SteamworksAppId { get; init; }

        public static SteamVentOptions Default { get; } = new SteamVentOptions();
    }
}