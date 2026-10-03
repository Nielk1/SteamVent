using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamVent;
using SteamVent.Core;
using Xunit;

namespace SteamVent.Tests.Orchestration
{
    // -- fakes ------------------------------------------------------------------
    sealed class FakeAppSource : ISource, IAppSource
    {
        public string Name { get; }
        public int Priority { get; }
        public SteamSource Family { get; }
        private readonly IReadOnlyCollection<OwnedAppInfo> _candidates;
        private readonly Func<uint, bool?> _owned;
        public FakeAppSource(string name, int priority, SteamSource family, IReadOnlyCollection<OwnedAppInfo> candidates, Func<uint, bool?> owned)
        {
            Name = name; Priority = priority; Family = family; _candidates = candidates; _owned = owned;
        }
        public bool IsAvailable() => true;
        public Task<bool> CheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<IReadOnlyCollection<OwnedAppInfo>> GetCandidateApps(CancellationToken cancellationToken = default) => Task.FromResult(_candidates);
        public Task<bool?> IsOwned(uint appId, CancellationToken cancellationToken = default) => Task.FromResult(_owned(appId));
    }

    sealed class FakeWorkshopSource : ISource, IWorkshopSource
    {
        public string Name { get; }
        public int Priority { get; }
        public SteamSource Family { get; }
        private readonly IReadOnlyCollection<PublishedFileData> _items;
        private readonly bool _throw;
        public FakeWorkshopSource(string name, int priority, SteamSource family, IReadOnlyCollection<PublishedFileData> items, bool throwOnError = false)
        {
            Name = name; Priority = priority; Family = family; _items = items; _throw = throwOnError;
        }
        public bool IsAvailable() => true;
        public Task<bool> CheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => _throw ? Task.FromException<IReadOnlyCollection<PublishedFileData>>(new Exception("boom")) : Task.FromResult(_items);
        public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => GetInstalledWorkshopItems(appid, cancellationToken);
    }

    public class SourcePipelineTests
    {
        private static SourcePipeline Make(params ISource[] sources)
            => new(new SteamVentOptions { Sources = SteamSource.All, SourceInstances = sources }, sources);

        [Fact]
        public async Task Candidates_are_unioned_and_deduplicated()
        {
            var a = new FakeAppSource("A", 10, SteamSource.Steamworks,
                new[] { new OwnedAppInfo(1, "One", "game", ""), new OwnedAppInfo(2, "Two", "game", "") }, _ => (bool?)null);
            var b = new FakeAppSource("B", 20, SteamSource.Steamworks,
                new[] { new OwnedAppInfo(2, "TwoDup", "game", ""), new OwnedAppInfo(3, "Three", "game", "") }, _ => (bool?)null);

            var result = await Make(a, b).GetCandidateAppsAsync();

            var ids = result.Select(r => r.AppId).OrderBy(x => x).ToArray();
            Assert.Equal(new[] { 1u, 2u, 3u }, ids);
            // A has lower priority, so its record for id 2 wins.
            Assert.Equal("Two", result.First(r => r.AppId == 2).Name);
        }

        [Fact]
        public async Task Ownership_first_non_null_wins_in_priority_order()
        {
            // High priority source can't decide (null) for app 9; lower priority says yes.
            var a = new FakeAppSource("A", 10, SteamSource.Steamworks, Array.Empty<OwnedAppInfo>(), id => id == 5 ? true : (bool?)null);
            var b = new FakeAppSource("B", 20, SteamSource.Steamworks, Array.Empty<OwnedAppInfo>(), id => id == 9 ? true : (bool?)null);

            var pipeline = Make(a, b);
            Assert.True(await pipeline.IsOwnedAsync(5));
            Assert.True(await pipeline.IsOwnedAsync(9));
            Assert.Null(await pipeline.IsOwnedAsync(42));
        }

        [Fact]
        public async Task A_source_that_throws_is_skipped_and_others_still_contribute()
        {
            var bad = new FakeWorkshopSource("Bad", 5, SteamSource.Bridge, new[] { new PublishedFileData(1, SteamVent.Core.Steamworks.EItemState.k_EItemStateInstalled) }, throwOnError: true);
            var good = new FakeWorkshopSource("Good", 30, SteamSource.FileSystem, new[] { new PublishedFileData(1, SteamVent.Core.Steamworks.EItemState.k_EItemStateInstalled), new PublishedFileData(7, SteamVent.Core.Steamworks.EItemState.k_EItemStateInstalled) });

            var result = await Make(bad, good).GetWorkshopItemsAsync(550);
            // The bad (higher-priority) source is skipped; the good one provides both ids.
            Assert.Contains(result, r => r.PublishedFileId == 7);
        }

        [Fact]
        public void A_disabled_family_is_excluded_from_the_pipeline()
        {
            var fs = new FakeWorkshopSource("FS", 30, SteamSource.FileSystem, Array.Empty<PublishedFileData>());
            var bridge = new FakeWorkshopSource("Bridge", 10, SteamSource.Bridge, Array.Empty<PublishedFileData>());

            // Only FileSystem enabled.
            var pipeline = new SourcePipeline(
                new SteamVentOptions { Sources = SteamSource.FileSystem, SourceInstances = new[] { fs, bridge } },
                new[] { fs, bridge });

            Assert.Contains(pipeline.Sources, s => s.Name == "FS");
            Assert.DoesNotContain(pipeline.Sources, s => s.Name == "Bridge");
        }
    }
}