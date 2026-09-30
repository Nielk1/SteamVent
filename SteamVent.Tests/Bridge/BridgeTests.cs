using SteamVent.Bridge;
using SteamVent.Common.InterProc;
using SteamVent.InterProc.Interfaces;

namespace SteamVent.Tests.Bridge;

public class BridgeTests
{
    [Fact]
    public async Task WorkshopItemSubscribeDownloadUnsubscribe()
    {
        UInt32 appId = UInt32.Parse(Core.Configuration["BridgeTests:AppID"]!);
        Assert.True(appId > 0, "InstalledAppID is missing from the test configuration.");

        // Penciled-in placeholder - swap in a real workshop item ID before running.
        UInt64 publishedFileId = UInt64.Parse(Core.Configuration["BridgeTests:PublishedFileID"]!);
        Assert.True(publishedFileId > 0, "PublishedFileID is missing from the test configuration.");

        using BridgeContext bridge = new(appId);

        RemoteStorageSubscribePublishedFileResult_t subscribed = await bridge.SteamWorkshopSubscribeAsync(publishedFileId);
        Assert.Equal(publishedFileId, subscribed.m_nPublishedFileId);
        Assert.True(
            subscribed.m_eResult is EResult.k_EResultOK or EResult.k_EResultDuplicateRequest,
            $"Subscribe to {publishedFileId} returned {subscribed.m_eResult}.");

        DownloadItemResult_t downloaded = await bridge.SteamWorkshopDownloadAsync(publishedFileId);
        Assert.Equal(publishedFileId, downloaded.m_nPublishedFileId);
        Assert.Equal(EResult.k_EResultOK, downloaded.m_eResult);

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
}
