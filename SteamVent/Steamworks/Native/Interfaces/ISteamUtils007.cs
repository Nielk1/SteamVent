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
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamUtils007 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("SteamUtils007")]
    public class ISteamUtils007 : SteamInterfaceWrapper, ISteamUtils
    {
        public ISteamUtils007(IntPtr interfacePtr) : base(interfacePtr) { }


        [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetAppIDDelegate(IntPtr thisPtr);
        public UInt32 GetAppID() => GetDelegate<GetAppIDDelegate>()(InterfacePtr);
        public FunctionImplemented GetAppIDSupport => FunctionImplemented.Implemented;


        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool IsAPICallCompletedDelegate(IntPtr thisPtr, UInt64 hSteamAPICall, ref bool pbFailed);
        public bool IsAPICallCompleted(UInt64 hSteamAPICall, ref bool pbFailed) => GetDelegate<IsAPICallCompletedDelegate>()(InterfacePtr, hSteamAPICall, ref pbFailed);
        public FunctionImplemented IsAPICallCompletedSupport => FunctionImplemented.Implemented;
    }
}
