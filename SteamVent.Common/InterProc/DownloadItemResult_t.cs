using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using PublishedFileId_t = System.UInt64;
using SteamVent.InterProc.Interfaces;

namespace SteamVent.Common.InterProc
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DownloadItemResult_t
    {
        public const int k_iCallback = 1354;
        [JsonInclude] public EResult m_eResult;
        [JsonInclude] public PublishedFileId_t m_nPublishedFileId;
    };
}
