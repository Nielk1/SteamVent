using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.SteamClient.Attributes;
using SteamVent.SteamClient.Interop;

namespace SteamVent.SteamClient.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamApps006 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMAPPS_INTERFACE_VERSION006")]
    public class ISteamApps006 : SteamInterfaceWrapper
    {
        public ISteamApps006(IntPtr interfacePtr) : base(interfacePtr) { }

        #region VTableIndex(0)
        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsSubscribedDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsSubscribed() => GetDelegate<BIsSubscribedDelegate>()(InterfacePtr);

        #region VTableIndex(1)
        [VTableIndex(1), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsLowViolenceDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsLowViolence() => GetDelegate<BIsLowViolenceDelegate>()(InterfacePtr);

        #region VTableIndex(2)
        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsCyberCafeDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsCyberCafe() => GetDelegate<BIsCyberCafeDelegate>()(InterfacePtr);

        #region VTableIndex(3)
        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsVACBannedDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsVACBanned() => GetDelegate<BIsVACBannedDelegate>()(InterfacePtr);

        #region VTableIndex(4)
        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetCurrentGameLanguageDelegate(IntPtr thisPtr);
        #endregion
        public string GetCurrentGameLanguage() => 
            DecodeUtf8String(GetDelegate<GetCurrentGameLanguageDelegate>()(InterfacePtr));

        #region VTableIndex(5)
        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetAvailableGameLanguagesDelegate(IntPtr thisPtr);
        #endregion
        public string GetAvailableGameLanguages() =>
            DecodeUtf8String(GetDelegate<GetAvailableGameLanguagesDelegate>()(InterfacePtr));

        #region VTableIndex(6)
        [VTableIndex(6), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsSubscribedAppDelegate(IntPtr thisPtr, UInt32 nAppId);
        #endregion
        public bool BIsSubscribedApp(UInt32 nAppId) => GetDelegate<BIsSubscribedAppDelegate>()(InterfacePtr, nAppId);

        #region VTableIndex(19)
        [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetIsAppInstalledDelegate(IntPtr thisPtr, UInt32 nAppId);
        #endregion
        public bool GetIsAppInstalled(UInt32 nAppId) => GetDelegate<GetIsAppInstalledDelegate>()(InterfacePtr, nAppId);
    }
}
