using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.Steamworks.Native.Attributes;
using SteamVent.Steamworks.Native;

namespace SteamVent.Steamworks.Native.Interfaces
{
    public interface ISteamUtils
    {
        bool IsAPICallCompleted(UInt64 hSteamAPICall, ref bool pbFailed);
    }
}
