using SteamVent.Core.Steamworks;

namespace SteamVent.Core
{
    /// <summary>
    /// A point-in-time sample of an in-flight workshop download: the item's live <see cref="EItemState"/> plus
    /// how much content has been transferred so far. Both the in-process (<c>SteamVent.Steamworks</c>) and the
    /// Bridge (<c>SteamVent.Bridge</c>) backends raise these (throttled to change) through an
    /// <c>IProgress&lt;WorkshopItemProgress&gt;</c>, so a caller can surface live transfer status without knowing
    /// which module answered. <see cref="Percent"/> is a convenience for progress bars (null until the total is
    /// known).
    /// </summary>
    public sealed record WorkshopItemProgress
    {
        /// <summary>The workshop item this sample is for (so a caller juggling several downloads can tell them apart).</summary>
        public ulong PublishedFileId { get; init; }

        /// <summary>The item's live state (e.g. <c>Downloading</c> / <c>DownloadPending</c> while in flight, <c>Installed</c> when done).</summary>
        public EItemState State { get; init; }

        /// <summary>Bytes transferred so far (0 until Steam reports progress).</summary>
        public ulong BytesDownloaded { get; init; }

        /// <summary>Total bytes to transfer (0 until Steam reports the size).</summary>
        public ulong BytesTotal { get; init; }

        /// <summary>0-100 completion percentage, or null until the total byte count is known.</summary>
        public double? Percent => BytesTotal > 0 ? (100.0 * BytesDownloaded) / BytesTotal : null;
    }
}