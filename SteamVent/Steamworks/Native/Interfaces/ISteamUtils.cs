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
        static readonly public Type[] Versions =
        {
            typeof(ISteamUtils007),
        };

        /// <summary>
        /// The AppID this Steamworks context is bound to - i.e. the app id of the running process. This is the
        /// source of truth for the active appid (as opposed to a manually configured value such as
        /// <see cref="SteamVent.Core.SteamVentOptions.SteamworksAppId"/>, which only <i>requests</i> a context
        /// before it starts).
        /// </summary>
        UInt32 GetAppID();
        FunctionImplemented GetAppIDSupport { get; }

        bool IsAPICallCompleted(UInt64 hSteamAPICall, ref bool pbFailed);
        FunctionImplemented IsAPICallCompletedSupport { get; }
    }
}
