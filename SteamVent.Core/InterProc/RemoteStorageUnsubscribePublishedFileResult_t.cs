using SteamVent.InterProc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PublishedFileId_t = System.UInt64;

namespace SteamVent.Core.InterProc
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct RemoteStorageUnsubscribePublishedFileResult_t
    {
        public const int k_iCallback = 1315;
        [JsonInclude] public EResult m_eResult;
        [JsonInclude] public PublishedFileId_t m_nPublishedFileId;
    };
}
