using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.BVdf;
using SteamVent.FileSystem;
using SteamVent.Steamworks.Native.Interfaces;

namespace SteamVent
{
    /// <summary>
    /// Enumerates apps from the Steam client's appcache (appinfo.vdf) - the ids of every app the client
    /// has ever seen. This is exactly why it is the best candidate source: it also contains betas, private
    /// games and developer items that never appear through conventional means. It is filesystem-only, so it
    /// is present whenever a Steam install can be located; it cannot authoritatively decide ownership.
    /// </summary>
    public sealed class FileSystemAppSource : ISource, IAppSource
    {
        public string Name => "FileSystem.Apps";
        public int Priority => 20;
        public SteamSource Family => SteamSource.FileSystem;

        private readonly object _lock = new object();
        private SteamAppInfoDataFile? _cache;
        private SteamAppInfoDataFile? Cache
        {
            get { lock (_lock) { _cache ??= SteamAppInfoDataFile.GetSteamAppInfoDataFile(); return _cache; } }
        }

        public bool IsAvailable() => !string.IsNullOrEmpty(SteamAppInfoDataFile.GetAppCacheAppInfoFile());
        public Task<bool> CheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(IsAvailable());

        public Task<IReadOnlyCollection<OwnedAppInfo>> GetCandidateApps(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildCandidates());

        /// <summary>Pure id source - ownership is decided by the Steamworks source.</summary>
        public Task<bool?> IsOwned(uint appId, CancellationToken cancellationToken = default)
            => Task.FromResult<bool?>(null);

        private IReadOnlyCollection<OwnedAppInfo> BuildCandidates()
        {
            var result = new List<OwnedAppInfo>();
            if (Cache?.chunks == null) return result;
            foreach (var chunk in Cache.chunks)
            {
                SteamAppInfo? app = SteamAppInfo.From(chunk);
                if (app?.Type == null || string.IsNullOrWhiteSpace(app.Type) || app.Type == "config") continue;

                result.Add(new OwnedAppInfo(app.AppId, app.Name ?? string.Empty, app.Type, app.IconPath ?? string.Empty));
            }
            return result;
        }
    }

    /// <summary>
    /// Authoritative ownership via the Steamworks ISteamApps interface - one <c>BIsSubscribedApp</c>
    /// spot-check per candidate id. This is what catches the answer that the id-only sources cannot
    /// provide. Returns null (not an error) when Steamworks is unavailable so the pipeline degrades.
    /// </summary>
    public sealed class SteamworksAppSource : ISource, IAppSource
    {
        public string Name => "Steamworks.Apps";
        public int Priority => 10; // checked first for ownership
        public SteamSource Family => SteamSource.Steamworks;

        private readonly Func<ISteamApps?> _apps;
        public SteamworksAppSource() : this(() => Steamworks.SteamClient.Construct().GetISteamApps()) { }
        public SteamworksAppSource(Func<ISteamApps?> appsProvider) { _apps = appsProvider; }

        public bool IsAvailable()
        {
            try { return Steamworks.SteamClient.Construct().IsAvailable; }
            catch { return false; }
        }
        public Task<bool> CheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(IsAvailable());

        public Task<IReadOnlyCollection<OwnedAppInfo>> GetCandidateApps(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<OwnedAppInfo>>(Array.Empty<OwnedAppInfo>());

        public Task<bool?> IsOwned(uint appId, CancellationToken cancellationToken = default)
        {
            try
            {
                ISteamApps? apps = _apps();
                return Task.FromResult<bool?>(apps == null ? null : apps.BIsSubscribedApp(appId));
            }
            catch
            {
                return Task.FromResult<bool?>(null);
            }
        }
    }
}