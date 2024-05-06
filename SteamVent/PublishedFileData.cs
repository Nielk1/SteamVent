using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent
{
    public class PublishedFileData
    {
        public ulong publishedFileId { get; set; }
        public InterProc.Interfaces.EItemState state { get; set; }
        public UInt64 punSizeOnDisk { get; set; }
        public string pchFolder { get; set; }
        public UInt32 punTimeStamp { get; set; }
        public UInt64 punBytesDownloaded { get; set; }
        public UInt64 punBytesTotal { get; set; }
        public PublishedFileData(ulong publishedFileId, InterProc.Interfaces.EItemState state)
        {
            this.publishedFileId = publishedFileId;
            this.state = state;
        }
    }
}
