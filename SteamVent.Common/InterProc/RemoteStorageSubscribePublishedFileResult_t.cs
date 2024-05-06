using SteamVent.InterProc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PublishedFileId_t = System.UInt64;

namespace SteamVent.Common.InterProc
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct RemoteStorageSubscribePublishedFileResult_t
    {
        public const int k_iCallback = 1313;
        public EResult m_eResult;
        public PublishedFileId_t m_nPublishedFileId;
    };
}
