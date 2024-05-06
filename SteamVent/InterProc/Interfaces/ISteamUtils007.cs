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
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamUtils007 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("SteamUtils007")]
    public class ISteamUtils007 : SteamInterfaceWrapper, ISteamUtils
    {
        public ISteamUtils007(IntPtr interfacePtr) : base(interfacePtr) { }

        #region VTableIndex(11)
        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool IsAPICallCompletedDelegate(IntPtr thisPtr, UInt64 hSteamAPICall, ref bool pbFailed);
        #endregion
        public bool IsAPICallCompleted(UInt64 hSteamAPICall, ref bool pbFailed) =>
            GetDelegate<IsAPICallCompletedDelegate>()(InterfacePtr, hSteamAPICall, ref pbFailed);
    }
}
