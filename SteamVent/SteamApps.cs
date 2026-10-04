using System;

namespace SteamVent
{
    /// <summary>
    /// The SteamVent <b>app</b> facade - a box over the in-process Steamworks app queries (ownership, install
    /// state, install directory). Obtain it from <see cref="SteamContext.SteamApps"/> rather than constructing it
    /// yourself: the context owns the underlying <see cref="Steamworks.SteamClient"/> and hands this facade the
    /// <see cref="Steamworks.SteamApps"/> it works over. You never see the raw client or the raw
    /// <see cref="Steamworks.SteamApps"/> - you just call the facade and it answers in accordance with the
    /// context's configuration.
    ///
    /// Every method is non-throwing: it returns <c>null</c> when the Steamworks interface is not available
    /// (Steam not running / not logged on), so callers can degrade gracefully.
    ///
    /// If you specifically want the raw child implementation, use <c>Steamworks.SteamApps</c> over a
    /// <c>Steamworks.SteamClient</c> you construct yourself.
    /// </summary>
    public sealed class SteamApps
    {
        private readonly Steamworks.SteamApps _inner;

        internal SteamApps(Steamworks.SteamApps inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <summary>True when the in-process app interface is reachable right now (Steam running and logged on).</summary>
        public bool IsAvailable
        {
            get { try { return _inner.GetAppInstalled(0) != null; } catch { return false; } }
        }

        /// <summary>Install directory for the app, or null when it is not installed / Steamworks unavailable.</summary>
        public string? GetAppInstallDir(uint appId) => _inner.GetAppInstallDir(appId);

        /// <summary>Whether the app is installed; null when Steamworks is unavailable.</summary>
        public bool? GetAppInstalled(uint appId) => _inner.GetAppInstalled(appId);

        /// <summary>Whether the current user owns (is subscribed to) the app; null when Steamworks is unavailable.</summary>
        public bool? IsSubscribed(uint appId) => _inner.IsSubscribed(appId);
    }
}