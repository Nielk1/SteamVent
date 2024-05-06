using SteamVent.Common.InterProc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Bridge
{
    public class PublishedFileData
    {
        public ulong publishedFileId { get; set; }
        public EItemState state { get; set; }
        public UInt64? punSizeOnDisk { get; set; }
        public string? pchFolder { get; set; }
        public UInt32? punTimeStamp { get; set; }
        public UInt64? punBytesDownloaded { get; set; }
        public UInt64? punBytesTotal { get; set; }
        public PublishedFileData(ulong publishedFileId, EItemState state)
        {
            this.publishedFileId = publishedFileId;
            this.state = state;
        }
    }
}
