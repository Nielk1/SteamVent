using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.InterProc.Attributes;
using SteamVent.InterProc.Interop;

namespace SteamVent.InterProc.Interfaces
{
    public interface ISteamUtils
    {
        bool IsAPICallCompleted(UInt64 hSteamAPICall, ref bool pbFailed);
    }
}
