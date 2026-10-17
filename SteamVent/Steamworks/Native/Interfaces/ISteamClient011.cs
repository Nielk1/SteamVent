using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.Steamworks.Native.Attributes;
using SteamVent.Steamworks.Native;
//using SteamVent.Tools;

namespace SteamVent.Steamworks.Native.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamClient011 (Version ID: 'SteamClient011') delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("SteamClient011")]
    public class ISteamClient011 : SteamInterfaceWrapper, ISteamClient
    {
        public ISteamClient011(IntPtr interfacePtr) : base(interfacePtr) { }

        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Int32 CreateSteamPipeDelegate(IntPtr thisPtr);
        public Int32 CreateSteamPipe() => GetDelegate<CreateSteamPipeDelegate>()(InterfacePtr);
        public FunctionImplemented CreateSteamPipeSupport => FunctionImplemented.Implemented;

        [VTableIndex(1), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BReleaseSteamPipeDelegate(IntPtr thisPtr, Int32 hSteamPipe);
        public bool BReleaseSteamPipe(Int32 hSteamPipe) => GetDelegate<BReleaseSteamPipeDelegate>()(InterfacePtr, hSteamPipe);
        public FunctionImplemented BReleaseSteamPipeSupport => FunctionImplemented.Implemented;

        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Int32 ConnectToGlobalUserDelegate(IntPtr thisPtr, Int32 hSteamPipe);
        public Int32 ConnectToGlobalUser(Int32 hSteamPipe) => GetDelegate<ConnectToGlobalUserDelegate>()(InterfacePtr, hSteamPipe);
        public FunctionImplemented ConnectToGlobalUserSupport => FunctionImplemented.Implemented;

        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void CreateLocalUserDelegate(IntPtr thisPtr, ref Int32 phSteamPipe, EAccountType eAccountType);
        public void CreateLocalUser(ref Int32 phSteamPipe, EAccountType eAccountType) => GetDelegate<CreateLocalUserDelegate>()(InterfacePtr, ref phSteamPipe, eAccountType);
        public FunctionImplemented CreateLocalUserSupport => FunctionImplemented.Implemented;
        public void CreateLocalUserLegacy(ref Int32 phSteamPipe) => CreateLocalUser(ref phSteamPipe, EAccountType.k_EAccountTypeIndividual);
        public FunctionImplemented CreateLocalUserLegacySupport => FunctionImplemented.Adapter;
        
        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void ReleaseUserDelegate(IntPtr thisPtr, Int32 hSteamPipe, Int32 hUser);
        public void ReleaseUser(Int32 hSteamPipe, Int32 hUser) => GetDelegate<ReleaseUserDelegate>()(InterfacePtr, hSteamPipe, hUser);
        public FunctionImplemented ReleaseUserSupport => FunctionImplemented.Implemented;

        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetISteamUserDelegate(IntPtr thisPtr, Int32 hSteamUser, Int32 hSteamPipe, IntPtr pchVersion);
        public TInterface GetISteamUser<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper => GetInterface<GetISteamUserDelegate, TInterface>(InterfacePtr, hSteamUser, hSteamPipe);
        public FunctionImplemented GetISteamUserSupport => FunctionImplemented.Implemented;

        [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetISteamUtilsDelegate(IntPtr thisPtr, Int32 hSteamPipe, IntPtr pchVersion);
        public TInterface GetISteamUtils<TInterface>(Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper => GetInterface<GetISteamUtilsDelegate, TInterface>(InterfacePtr, hSteamPipe);
        public FunctionImplemented GetISteamUtilsSupport => FunctionImplemented.Implemented;

        [VTableIndex(16), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetISteamAppsDelegate(IntPtr thisPtr, Int32 hSteamUser, Int32 hSteamPipe, IntPtr pchVersion);
        public TInterface GetISteamApps<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper => GetInterface<GetISteamAppsDelegate, TInterface>(InterfacePtr, hSteamUser, hSteamPipe);
        public FunctionImplemented GetISteamAppsSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public TInterface GetISteamUGC<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper { throw new NotImplementedException(); }
        public FunctionImplemented GetISteamUGCSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(7), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetLocalIPBindingDelegate(IntPtr thisPtr, UInt32 unIP, UInt16 usPort);
        public void SetLocalIPBinding(UInt32 unIP, UInt16 usPort) => GetDelegate<SetLocalIPBindingDelegate>()(InterfacePtr, unIP, usPort);
        public FunctionImplemented SetLocalIPBindingSupport => FunctionImplemented.Implemented;

        [VTableIndex(20), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void RunFrameDelegate(IntPtr thisPtr);
        public void RunFrame() => GetDelegate<RunFrameDelegate>()(InterfacePtr);
        public FunctionImplemented RunFrameSupport => FunctionImplemented.Implemented;

        [VTableIndex(22), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetWarningMessageHookDelegate(IntPtr thisPtr, IntPtr pFunction);
        public void SetWarningMessageHook(SteamAPIWarningMessageHook_t pFunction) => GetDelegate<SetWarningMessageHookDelegate>()(InterfacePtr, Marshal.GetFunctionPointerForDelegate(pFunction));
        public FunctionImplemented SetWarningMessageHookSupport => FunctionImplemented.Implemented;

        [VTableIndex(23), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BShutdownIfAllPipesClosedDelegate(IntPtr thisPtr);
        public bool BShutdownIfAllPipesClosed() => GetDelegate<BShutdownIfAllPipesClosedDelegate>()(InterfacePtr);
        public FunctionImplemented BShutdownIfAllPipesClosedSupport => FunctionImplemented.Implemented;
    }
}
