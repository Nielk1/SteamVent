using System;
using SteamVent.Core.Steamworks;

namespace SteamVent.Core
{
    /// <summary>
    /// A workshop item and its install / subscription status, as reported by a source.
    ///
    /// This is the shared workshop-item model. It lives in SteamVent.Core (not SteamVent.Bridge) so
    /// that the Bridge client, the SteamVentContextBridge child process, SteamCmd, Steamworks and the
    /// FileSystem source all speak the same model - and so optional modules can degrade out of the
    /// picture entirely without Core ever referencing them.
    /// </summary>
    public class PublishedFileData
    {
        public PublishedFileData() { }

        public PublishedFileData(ulong publishedFileId, EItemState state)
        {
            PublishedFileId = publishedFileId;
            State = state;
        }

        public ulong PublishedFileId { get; set; }

        /// <summary>Bitmask of <see cref="EItemState"/> describing the item's local state.</summary>
        public EItemState State { get; set; }

        /// <summary>Unix timestamp (seconds) the content was last modified by its creator.</summary>
        public ulong TimeStamp { get; set; }

        /// <summary>Size of the installed content on disk, in bytes (0 when not installed).</summary>
        public ulong SizeOnDisk { get; set; }

        /// <summary>Install folder for the item (empty when not installed).</summary>
        public string? Folder { get; set; }

        /// <summary>Bytes downloaded so far, when an update is in progress.</summary>
        public ulong BytesDownloaded { get; set; }

        /// <summary>Total bytes to download, when an update is in progress.</summary>
        public ulong BytesTotal { get; set; }

        public bool IsSubscribed => State.HasFlag(EItemState.k_EItemStateSubscribed);
        public bool IsDownloaded => State.HasFlag(EItemState.k_EItemStateInstalled);
    }
}