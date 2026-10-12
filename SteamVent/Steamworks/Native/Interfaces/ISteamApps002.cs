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
    /// Contains ISteamApps002 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMAPPS_INTERFACE_VERSION002")]
    public class ISteamApps002 : SteamInterfaceWrapper, ISteamApps
    {
        public ISteamApps002(IntPtr interfacePtr) : base(interfacePtr) { }

        #region VTableIndex(0)
        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsSubscribedDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsSubscribed() =>
            GetDelegate<BIsSubscribedDelegate>()(InterfacePtr);

        #region VTableIndex(1)
        [VTableIndex(1), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsLowViolenceDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsLowViolence() =>
            GetDelegate<BIsLowViolenceDelegate>()(InterfacePtr);

        #region VTableIndex(2)
        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsCyberCafeDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsCyberCafe() =>
            GetDelegate<BIsCyberCafeDelegate>()(InterfacePtr);

        #region VTableIndex(3)
        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsVACBannedDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsVACBanned() =>
            GetDelegate<BIsVACBannedDelegate>()(InterfacePtr);

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
        public bool BIsSubscribedApp(UInt32 nAppId) =>
            GetDelegate<BIsSubscribedAppDelegate>()(InterfacePtr, nAppId);

        [Obsolete("Not implemented in this version.", true)] public int BIsDlcInstalled(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetEarliestPurchaseUnixTime(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool BIsSubscribedFromFreeWeekend() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public int GetDLCCount() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool BGetDLCDataByIndex(Int32 nDLCIndex, ref UInt32 pnAppID, StringBuilder pchName, Int32 cchNameBufferSize, ref bool pbAvailable) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public void InstallDLC(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public void UninstallDLC(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public void RequestAppProofOfPurchaseKey(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public string GetCurrentBetaName() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public void MarkContentCorrupt(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public void GetInstalledDepots(UInt32 nAppId, UInt64[] pvecDepots, ref Int32 pnCount) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public string GetAppInstallDir(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool BIsAppInstalled(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public UInt64 GetAppOwner(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public string GetLaunchQueryParam(string pchKey) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public void GetDlcDownloadProgress(UInt32 nAppId, ref UInt64 pnBytesDownloaded, ref UInt64 pnBytesTotal) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetAppBuildId(UInt32 nAppId) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public void RequestAllProofOfPurchaseKeys() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetFileDetails(string pszFileName) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public string GetLaunchCommandLine() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool BIsSubscribedFromFamilySharing() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool BIsTimedTrial(ref UInt32 punSecondsAllowed, ref UInt32 punSecondsPlayed) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool SetDlcContext(UInt32 nAppId) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public Int32 GetNumBetas(ref Int32 pnAvailable, ref Int32 pnPrivate) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool GetBetaInfo(Int32 iBetaIndex, ref UInt32 punFlags, ref UInt32 punBuildID, StringBuilder pchBetaName, Int32 cchBetaName, StringBuilder pchDescription, Int32 cchDescription, ref UInt32 punLastUpdated) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool SetActiveBeta(string pchBetaName) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public void SetGamePerformanceSetting(EGamePerformanceSetting setting) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public void SetGameRenderResolution(UInt32 unWidth, UInt32 unHeight) { throw new NotImplementedException(); }
    }
}
