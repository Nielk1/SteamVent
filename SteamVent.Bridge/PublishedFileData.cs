using SteamVent.Common.InterProc;

namespace SteamVent.Bridge;

/// <summary>
/// The subscription/install state of a single workshop item, as reported by the bridge.
/// </summary>
public sealed class PublishedFileData
{
    public PublishedFileData()
    {
    }

    public PublishedFileData(ulong publishedFileId, EItemState state)
    {
        PublishedFileId = publishedFileId;
        State = state;
    }

    public ulong PublishedFileId { get; set; }
    public EItemState State { get; set; }
    public UInt64? SizeOnDisk { get; set; }
    public string? Folder { get; set; }
    public UInt32? TimeStamp { get; set; }
    public UInt64? BytesDownloaded { get; set; }
    public UInt64? BytesTotal { get; set; }
}
