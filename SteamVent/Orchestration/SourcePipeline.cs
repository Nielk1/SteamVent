using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;

namespace SteamVent
{
    /// <summary>
    /// Discovers <see cref="ISource"/> implementations from a set of assemblies with NO compile-time
    /// reference to them. This is the mechanism that makes Bridge / SteamCmd removable: we reflect over
    /// whatever is actually loaded and instantiate only the sources that are present - a missing
    /// assembly is simply absent from the results instead of throwing.
    /// </summary>
    public static class SourceDiscovery
    {
        public static IReadOnlyList<ISource> Discover(IEnumerable<Assembly>? assemblies, Func<ISource, bool>? predicate = null)
        {
            var sources = new List<ISource>();
            var seen = new HashSet<Type>();
            foreach (var assembly in assemblies ?? Array.Empty<Assembly>())
            {
                Type[] types;
                try { types = assembly.GetExportedTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray()!; }
                catch { continue; }

                foreach (var type in types)
                {
                    if (type == null || !seen.Add(type)) continue;
                    if (!typeof(ISource).IsAssignableFrom(type) || type.IsAbstract || !type.IsPublic) continue;
                    if (type.GetConstructor(Type.EmptyTypes) == null) continue;
                    try
                    {
                        if (Activator.CreateInstance(type) is ISource instance && (predicate == null || predicate(instance)))
                            sources.Add(instance);
                    }
                    catch { /* a source that fails to construct is skipped - graceful degradation */ }
                }
            }
            return sources;
        }
    }

    /// <summary>
    /// Composes a set of <see cref="ISource"/> providers into one degrading query surface. Sources are
    /// ordered by priority and each individual failure (missing assembly, timeout, exception) is
    /// swallowed so the library degrades to the sources that are actually available instead of throwing.
    /// </summary>
    public sealed class SourcePipeline : IDisposable
    {
        private readonly List<ISource> _sources;
        public SteamVentOptions Options { get; }
        public IReadOnlyList<ISource> Sources => _sources;

        public SourcePipeline(SteamVentOptions options, IEnumerable<ISource> sources)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _sources = (sources ?? Array.Empty<ISource>())
                .Where(s => Options.Sources.HasFlag(s.Family))
                .OrderBy(s => s.Priority).ThenBy(s => s.Name, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>Union of every app id a candidate source can see (deduplicated).</summary>
        public async Task<IReadOnlyCollection<OwnedAppInfo>> GetCandidateAppsAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<OwnedAppInfo>();
            var seen = new HashSet<uint>();
            foreach (var source in _sources)
            {
                if (source is not IAppSource app) continue;
                using var cts = WithTimeout(cancellationToken);
                try
                {
                    foreach (var item in await app.GetCandidateApps(cts.Token))
                        if (seen.Add(item.AppId)) result.Add(item);
                }
                catch (OperationCanceledException) { throw; }
                catch { /* degrade: skip this source and continue */ }
            }
            return result;
        }

        /// <summary>
        /// Ownership for one app: the first source (in priority order) that can answer it wins. Returns
        /// null when no available source can determine ownership (e.g. Steamworks is down).
        /// </summary>
        public async Task<bool?> IsOwnedAsync(uint appId, CancellationToken cancellationToken = default)
        {
            foreach (var source in _sources)
            {
                if (source is not IAppSource app) continue;
                using var cts = WithTimeout(cancellationToken);
                try
                {
                    var owned = await app.IsOwned(appId, cts.Token);
                    if (owned.HasValue) return owned;
                }
                catch (OperationCanceledException) { throw; }
                catch { /* degrade: try the next source */ }
            }
            return null;
        }

        /// <summary>
        /// Merged workshop items for an appid. The highest-priority source that reports an id provides
        /// that id's record; lower-priority sources only fill in ids no higher-priority source reported.
        /// </summary>
        public async Task<IReadOnlyCollection<PublishedFileData>> GetWorkshopItemsAsync(int appid, CancellationToken cancellationToken = default)
        {
            var byId = new Dictionary<ulong, PublishedFileData>();
            foreach (var source in _sources)
            {
                if (source is not IWorkshopSource workshop) continue;
                using var cts = WithTimeout(cancellationToken);
                try
                {
                    foreach (var item in await workshop.GetInstalledWorkshopItems(appid, cts.Token))
                        if (item != null && !byId.ContainsKey(item.PublishedFileId))
                            byId[item.PublishedFileId] = item;
                }
                catch (OperationCanceledException) { throw; }
                catch { /* degrade: skip this source */ }
            }
            return byId.Values;
        }

        /// <summary>The best available appid-context workshop command provider (Bridge, else SteamCmd), or null.</summary>
        public IWorkshopCommands? GetWorkshopCommands()
            => _sources.OfType<IWorkshopCommands>().FirstOrDefault();

        private CancellationTokenSource WithTimeout(CancellationToken outer)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(outer);
            if (Options.SourceTimeout > TimeSpan.Zero)
                cts.CancelAfter(Options.SourceTimeout);
            return cts;
        }

        public void Dispose()
        {
            foreach (var source in _sources) (source as IDisposable)?.Dispose();
        }
    }
}