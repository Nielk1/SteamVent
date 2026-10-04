using System;

namespace SteamVent
{
    /// <summary>
    /// The SteamVent <b>app</b> facade - a box over the in-process Steamworks app queries (ownership, install
    /// state, install directory). Obtain it from <see cref="SteamContext.SteamApps"/>. Every method is
    /// non-throwing: it returns <c>null</c> when the Steamworks subsystem is disabled or unavailable (Steam not
    /// running / not logged on), so callers degrade gracefully.
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

        /// <summary>Install directory for the app, or null when it is not installed / Steamworks unavailable.</summary>
        public string? GetAppInstallDir(uint appId) => Inner?.GetAppInstallDir(appId);

        /// <summary>Whether the app is installed; null when Steamworks is unavailable.</summary>
        public bool? GetAppInstalled(uint appId) => Inner?.GetAppInstalled(appId);

        /// <summary>Whether the current user owns (is subscribed to) the app; null when Steamworks is unavailable.</summary>
        public bool? IsSubscribed(uint appId) => Inner?.IsSubscribed(appId);
    }
}