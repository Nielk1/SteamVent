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
    /// Contains ISteamApps006 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMAPPS_INTERFACE_VERSION006")]
    public class ISteamApps006 : SteamInterfaceWrapper, ISteamApps
    {
        public ISteamApps006(IntPtr interfacePtr) : base(interfacePtr) { }

        [Obsolete("Not implemented in this version.", true)] public int GetAppData(UInt32 nAppId, string pchKey, StringBuilder pchValue, int cchValueMax) { throw new NotImplementedException(); }
        public FunctionImplemented GetAppDataSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsSubscribedDelegate(IntPtr thisPtr);
        public bool BIsSubscribed() => GetDelegate<BIsSubscribedDelegate>()(InterfacePtr);
        public FunctionImplemented BIsSubscribedSupport => FunctionImplemented.Implemented;

        [VTableIndex(1), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsLowViolenceDelegate(IntPtr thisPtr);
        public bool BIsLowViolence() => GetDelegate<BIsLowViolenceDelegate>()(InterfacePtr);
        public FunctionImplemented BIsLowViolenceSupport => FunctionImplemented.Implemented;

        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsCyberCafeDelegate(IntPtr thisPtr);
        public bool BIsCyberCafe() => GetDelegate<BIsCyberCafeDelegate>()(InterfacePtr);
        public FunctionImplemented BIsCyberCafeSupport => FunctionImplemented.Implemented;

        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsVACBannedDelegate(IntPtr thisPtr);
        public bool BIsVACBanned() => GetDelegate<BIsVACBannedDelegate>()(InterfacePtr);
        public FunctionImplemented BIsVACBannedSupport => FunctionImplemented.Implemented;

        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetCurrentGameLanguageDelegate(IntPtr thisPtr);
        public string GetCurrentGameLanguage() => DecodeUtf8String(GetDelegate<GetCurrentGameLanguageDelegate>()(InterfacePtr));
        public FunctionImplemented GetCurrentGameLanguageSupport => FunctionImplemented.Implemented;

        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetAvailableGameLanguagesDelegate(IntPtr thisPtr);
        public string GetAvailableGameLanguages() => DecodeUtf8String(GetDelegate<GetAvailableGameLanguagesDelegate>()(InterfacePtr));
        public FunctionImplemented GetAvailableGameLanguagesSupport => FunctionImplemented.Implemented;

        [VTableIndex(6), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsSubscribedAppDelegate(IntPtr thisPtr, UInt32 nAppId);
        public bool BIsSubscribedApp(UInt32 nAppId) => GetDelegate<BIsSubscribedAppDelegate>()(InterfacePtr, nAppId);
        public FunctionImplemented BIsSubscribedAppSupport => FunctionImplemented.Implemented;

        [VTableIndex(7), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int BIsDlcInstalledDelegate(IntPtr thisPtr, UInt32 nAppId);
        public int BIsDlcInstalled(UInt32 nAppId) => GetDelegate<BIsDlcInstalledDelegate>()(InterfacePtr, nAppId);
        public FunctionImplemented BIsDlcInstalledSupport => FunctionImplemented.Implemented;

        [VTableIndex(8), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetEarliestPurchaseUnixTimeDelegate(IntPtr thisPtr, UInt32 nAppId);
        public UInt32 GetEarliestPurchaseUnixTime(UInt32 nAppId) => GetDelegate<GetEarliestPurchaseUnixTimeDelegate>()(InterfacePtr, nAppId);
        public FunctionImplemented GetEarliestPurchaseUnixTimeSupport => FunctionImplemented.Implemented;

        [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsSubscribedFromFreeWeekendDelegate(IntPtr thisPtr);
        public bool BIsSubscribedFromFreeWeekend() => GetDelegate<BIsSubscribedFromFreeWeekendDelegate>()(InterfacePtr);
        public FunctionImplemented BIsSubscribedFromFreeWeekendSupport => FunctionImplemented.Implemented;

        [VTableIndex(10), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetDLCCountDelegate(IntPtr thisPtr);
        public int GetDLCCount() => GetDelegate<GetDLCCountDelegate>()(InterfacePtr);
        public FunctionImplemented GetDLCCountSupport => FunctionImplemented.Implemented;

        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BGetDLCDataByIndexDelegate(IntPtr thisPtr, Int32 iDLC, ref UInt32 pAppID, ref bool pbAvailable, StringBuilder pchName, Int32 cchNameBufferSize);
        public bool BGetDLCDataByIndex(Int32 nDLCIndex, ref UInt32 pnAppID, StringBuilder pchName, Int32 cchNameBufferSize, ref bool pbAvailable) => GetDelegate<BGetDLCDataByIndexDelegate>()(InterfacePtr, nDLCIndex, ref pnAppID, ref pbAvailable, pchName, cchNameBufferSize);
        public FunctionImplemented BGetDLCDataByIndexSupport => FunctionImplemented.Implemented;

        [VTableIndex(12), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InstallDLCDelegate(IntPtr thisPtr, UInt32 nAppId);
        public void InstallDLC(UInt32 nAppId) => GetDelegate<InstallDLCDelegate>()(InterfacePtr, nAppId);
        public FunctionImplemented InstallDLCSupport => FunctionImplemented.Implemented;

        [VTableIndex(13), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void UninstallDLCDelegate(IntPtr thisPtr, UInt32 nAppId);
        public void UninstallDLC(UInt32 nAppId) => GetDelegate<UninstallDLCDelegate>()(InterfacePtr, nAppId);
        public FunctionImplemented UninstallDLCSupport => FunctionImplemented.Implemented;

        [VTableIndex(14), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void RequestAppProofOfPurchaseKeyDelegate(IntPtr thisPtr, UInt32 nAppID);
        public void RequestAppProofOfPurchaseKey(UInt32 nAppId) => GetDelegate<RequestAppProofOfPurchaseKeyDelegate>()(InterfacePtr, nAppId);
        public FunctionImplemented RequestAppProofOfPurchaseKeySupport => FunctionImplemented.Implemented;

        [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetCurrentBetaNameDelegate(IntPtr thisPtr, IntPtr pchName, Int32 cchNameBufferSize);
        public string GetCurrentBetaName()
        {
            const Int32 size = 256;
            IntPtr pchName = Marshal.AllocHGlobal(size + 1);
            try
            {
                GetDelegate<GetCurrentBetaNameDelegate>()(InterfacePtr, pchName, size);
                return DecodeUtf8String(pchName);
            }
            finally
            {
                Marshal.FreeHGlobal(pchName);
            }
        }
        public FunctionImplemented GetCurrentBetaNameSupport => FunctionImplemented.Implemented;

        [VTableIndex(16), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void MarkContentCorruptDelegate(IntPtr thisPtr, UInt32 nAppID);
        public void MarkContentCorrupt(UInt32 nAppId) => GetDelegate<MarkContentCorruptDelegate>()(InterfacePtr, nAppId);
        public FunctionImplemented MarkContentCorruptSupport => FunctionImplemented.Implemented;

        [VTableIndex(17), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void GetInstalledDepotsDelegate(IntPtr thisPtr, UInt32 nAppID, UInt64[] pvecDepots, ref Int32 pnCount);
        public void GetInstalledDepots(UInt32 nAppId, UInt64[] pvecDepots, ref Int32 pnCount) => GetDelegate<GetInstalledDepotsDelegate>()(InterfacePtr, nAppId, pvecDepots, ref pnCount);
        public FunctionImplemented GetInstalledDepotsSupport => FunctionImplemented.Implemented;

        [VTableIndex(18), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetAppInstallDirDelegate(IntPtr thisPtr, UInt32 nAppId, IntPtr pchFolder, UInt32 cchFolderBufferSize);
        public string GetAppInstallDir(UInt32 nAppId)
        {
            UInt32 size = 1024;
            byte[] _pchFolder = new byte[size];
            IntPtr pchFolder = Marshal.AllocHGlobal(_pchFolder.Length + 1);
            UInt32 read = GetDelegate<GetAppInstallDirDelegate>()(InterfacePtr, nAppId, pchFolder, size);
            Marshal.Copy(pchFolder, _pchFolder, 0, (int)read);
            return Encoding.UTF8.GetString(_pchFolder, 0, (int)read);
        }
        public FunctionImplemented GetAppInstallDirSupport => FunctionImplemented.Implemented;

        [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetIsAppInstalledDelegate(IntPtr thisPtr, UInt32 nAppId);
        public bool BIsAppInstalled(UInt32 nAppId) => GetDelegate<GetIsAppInstalledDelegate>()(InterfacePtr, nAppId);
        public FunctionImplemented BIsAppInstalledSupport => FunctionImplemented.Implemented;

        [VTableIndex(20), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 GetAppOwnerDelegate(IntPtr thisPtr);
        public UInt64 GetAppOwner(UInt32 nAppId) => GetDelegate<GetAppOwnerDelegate>()(InterfacePtr);
        public FunctionImplemented GetAppOwnerSupport => FunctionImplemented.Implemented;

        [VTableIndex(21), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetLaunchQueryParamDelegate(IntPtr thisPtr, IntPtr pchKey);
        public string GetLaunchQueryParam(string pchKey)
        {
            IntPtr keyUtf8 = Marshal.StringToCoTaskMemUTF8(pchKey);
            try
            {
                return DecodeUtf8String(GetDelegate<GetLaunchQueryParamDelegate>()(InterfacePtr, keyUtf8));
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUTF8(keyUtf8);
            }
        }
        public FunctionImplemented GetLaunchQueryParamSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public void GetDlcDownloadProgress(UInt32 nAppId, ref UInt64 pnBytesDownloaded, ref UInt64 pnBytesTotal) { throw new NotImplementedException(); }
        public FunctionImplemented GetDlcDownloadProgressSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetAppBuildId(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented GetAppBuildIdSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void RequestAllProofOfPurchaseKeys() { throw new NotImplementedException(); }
        public FunctionImplemented RequestAllProofOfPurchaseKeysSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetFileDetails(string pszFileName) { throw new NotImplementedException(); }
        public FunctionImplemented GetFileDetailsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public string GetLaunchCommandLine() { throw new NotImplementedException(); }
        public FunctionImplemented GetLaunchCommandLineSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BIsSubscribedFromFamilySharing() { throw new NotImplementedException(); }
        public FunctionImplemented BIsSubscribedFromFamilySharingSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BIsTimedTrial(ref UInt32 punSecondsAllowed, ref UInt32 punSecondsPlayed) { throw new NotImplementedException(); }
        public FunctionImplemented BIsTimedTrialSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetDlcContext(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented SetDlcContextSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public Int32 GetNumBetas(ref Int32 pnAvailable, ref Int32 pnPrivate) { throw new NotImplementedException(); }
        public FunctionImplemented GetNumBetasSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetBetaInfo(Int32 iBetaIndex, ref UInt32 punFlags, ref UInt32 punBuildID, StringBuilder pchBetaName, Int32 cchBetaName, StringBuilder pchDescription, Int32 cchDescription, ref UInt32 punLastUpdated) { throw new NotImplementedException(); }
        public FunctionImplemented GetBetaInfoSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetActiveBeta(string pchBetaName) { throw new NotImplementedException(); }
        public FunctionImplemented SetActiveBetaSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void SetGamePerformanceSetting(EGamePerformanceSetting setting) { throw new NotImplementedException(); }
        public FunctionImplemented SetGamePerformanceSettingSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void SetGameRenderResolution(UInt32 unWidth, UInt32 unHeight) { throw new NotImplementedException(); }
        public FunctionImplemented SetGameRenderResolutionSupport => FunctionImplemented.NotImplemented;
    }
}
