using System.Threading;
using SteamVent.Bridge;
using SteamVent.Core;
using SteamVent.Core.Steamworks;
using SteamVent.Steamworks.Native.Interfaces;

namespace SteamVent.Tests.Bridge;

public class BridgeTests
{
    private static (UInt32 AppId, UInt64 PublishedFileId) ReadConfig()
    {
        UInt32 appId = UInt32.Parse(Core.Configuration["BridgeTests:AppID"]!);
        Assert.True(appId > 0, "InstalledAppID is missing from the test configuration.");

        // Penciled-in placeholder - swap in a real workshop item ID before running.
        UInt64 publishedFileId = UInt64.Parse(Core.Configuration["BridgeTests:PublishedFileID"]!);
        Assert.True(publishedFileId > 0, "PublishedFileID is missing from the test configuration.");

        return (appId, publishedFileId);
    }

    [Fact]
    public async Task WorkshopItemSubscribeDownloadUnsubscribe()
    {
        (UInt32 appId, UInt64 publishedFileId) = ReadConfig();

        using BridgeContext bridge = new(appId);
        try
        {
            await RunRoundTrip(bridge, publishedFileId);
        }
        catch (WorkshopException ex) when (ex.IsAppIdMismatch || ex.Message.Contains("running under AppID", StringComparison.Ordinal))
        {
            // This is a live-Steam test: it needs Steam actually running under the configured app context. The
            // AppID-context guard now fails loudly when it isn't (e.g. Steam reports the shell / a different app)
            // instead of silently acting on the wrong app's workshop, so we skip rather than run against the wrong
            // context when the environment cannot provide the requested app.
            Assert.Skip($"Requires Steam running under AppID {appId} (IsAppIdMismatch={ex.IsAppIdMismatch}); got: {ex.Message}");
        }
    }

    private static async Task RunRoundTrip(BridgeContext bridge, UInt64 publishedFileId)
    {
        RemoteStorageSubscribePublishedFileResult_t subscribed = await bridge.SteamWorkshopSubscribeAsync(publishedFileId);
        Assert.Equal(publishedFileId, subscribed.m_nPublishedFileId);
        Assert.True(
            subscribed.m_eResult is EResult.k_EResultOK or EResult.k_EResultDuplicateRequest,
            $"Subscribe to {publishedFileId} returned {subscribed.m_eResult}.");

        // Exercise the shared progress surface. Sample count is timing-dependent (0 for an already-current item),
        // so we only assert the callback contract (the item id) rather than a specific count.
        int sampleCount = 0;
        ulong? observedId = null;
        IProgress<WorkshopItemProgress> progress = new Progress<WorkshopItemProgress>(sample =>
        {
            Interlocked.Increment(ref sampleCount);
            observedId = sample.PublishedFileId;
        });

        DownloadItemResult_t downloaded = await bridge.SteamWorkshopDownloadAsync(publishedFileId, progress);
        Assert.Equal(publishedFileId, downloaded.m_nPublishedFileId);
        Assert.Equal(EResult.k_EResultOK, downloaded.m_eResult);
        if (observedId is { } id)
            Assert.Equal(publishedFileId, id);

        PublishedFileData[] items = await bridge.SteamWorkshopListAsync();
        PublishedFileData? item = items.FirstOrDefault(i => i.PublishedFileId == publishedFileId);
        Assert.NotNull(item);
        Assert.True(
            item!.State.HasFlag(EItemState.k_EItemStateInstalled),
            $"Item {publishedFileId} is not reported as installed (state: {item.State}).");

        RemoteStorageUnsubscribePublishedFileResult_t unsubscribed = await bridge.SteamWorkshopUnsubscribeAsync(publishedFileId);
        Assert.Equal(publishedFileId, unsubscribed.m_nPublishedFileId);
        Assert.True(
            unsubscribed.m_eResult is EResult.k_EResultOK or EResult.k_EResultNoMatch,
            $"Unsubscribe from {publishedFileId} returned {unsubscribed.m_eResult}.");
    }

    [Fact]
    public void BridgePoolReusesTheSameContextForTheSameAppId()
    {
        (UInt32 appId, _) = ReadConfig();

        using BridgePool pool = new(TimeSpan.FromMinutes(1));

        BridgeContext first = pool.Acquire(appId);
        try
        {
            BridgeContext second = pool.Acquire(appId);
            Assert.True(ReferenceEquals(first, second), "A second Acquire for the same appid should reuse the warm bridge, not spawn a new one.");
            pool.Release(appId);
            pool.Release(appId);
        }
        finally
        {
            pool.Dispose();
        }
    }
}
