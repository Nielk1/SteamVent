using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.SteamClient.Attributes;
using SteamVent.SteamClient.Interop;
using SteamVent.Tools;

namespace SteamVent.SteamClient.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamClient017 (Version ID: 'SteamClient017') delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("SteamClient017")]
    public class ISteamClient017 : SteamInterfaceWrapper, ISteamClient
    {
        public ISteamClient017(IntPtr interfacePtr) : base(interfacePtr) { }

        #region VTableIndex(0)
        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Int32 CreateSteamPipeDelegate(IntPtr thisPtr);
        #endregion
        public Int32 CreateSteamPipe() => GetDelegate<CreateSteamPipeDelegate>()(InterfacePtr);

        #region VTableIndex(1)
        [VTableIndex(1), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BReleaseSteamPipeDelegate(IntPtr thisPtr, Int32 hSteamPipe);
        #endregion
        public bool BReleaseSteamPipe(Int32 hSteamPipe) =>
            GetDelegate<BReleaseSteamPipeDelegate>()(InterfacePtr, hSteamPipe);

        #region VTableIndex(2)
        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Int32 ConnectToGlobalUserDelegate(IntPtr thisPtr, Int32 hSteamPipe);
        #endregion
        public Int32 ConnectToGlobalUser(Int32 hSteamPipe) =>
            GetDelegate<ConnectToGlobalUserDelegate>()(InterfacePtr, hSteamPipe);

        #region VTableIndex(3)
        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void CreateLocalUserDelegate(IntPtr thisPtr, ref Int32 phSteamPipe, EAccountType eAccountType);
        #endregion
        public void CreateLocalUser(ref Int32 phSteamPipe, EAccountType eAccountType) =>
            GetDelegate<CreateLocalUserDelegate>()(InterfacePtr, ref phSteamPipe, eAccountType);

        #region VTableIndex(4)
        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void ReleaseUserDelegate(IntPtr thisPtr, Int32 hSteamPipe, Int32 hUser);
        #endregion
        public void ReleaseUser(Int32 hSteamPipe, Int32 hUser) =>
            GetDelegate<ReleaseUserDelegate>()(InterfacePtr, hSteamPipe, hUser);

        #region VTableIndex(5)
        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetISteamUserDelegate(IntPtr thisPtr, Int32 hSteamUser, Int32 hSteamPipe, string pchVersion);
        #endregion
        public TInterface GetISteamUser<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper =>
            GetInterface<GetISteamAppsDelegate, TInterface>(InterfacePtr, hSteamUser, hSteamPipe);

        #region VTableIndex(15)
        [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetISteamAppsDelegate(IntPtr thisPtr, Int32 hSteamUser, Int32 hSteamPipe, string pchVersion);
        #endregion
        public TInterface GetISteamApps<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper =>
            GetInterface<GetISteamAppsDelegate, TInterface>(InterfacePtr, hSteamUser, hSteamPipe);
    }
}
