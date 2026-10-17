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
    /// Contains ISteamApps005 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMAPPS_INTERFACE_VERSION005")]
    public class ISteamApps005 : SteamInterfaceWrapper, ISteamApps
    {
        public ISteamApps005(IntPtr interfacePtr) : base(interfacePtr) { }

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

        //GetCurrentBetaName
        //MarkContentCorrupt
        //GetInstalledDepots

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

        [Obsolete("Not implemented in this version.", true)] public string GetCurrentBetaName() { throw new NotImplementedException(); }
        public FunctionImplemented GetCurrentBetaNameSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void MarkContentCorrupt(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented MarkContentCorruptSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void GetInstalledDepots(UInt32 nAppId, UInt64[] pvecDepots, ref Int32 pnCount) { throw new NotImplementedException(); }
        public FunctionImplemented GetInstalledDepotsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 GetAppOwner(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented GetAppOwnerSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public string GetLaunchQueryParam(string pchKey) { throw new NotImplementedException(); }
        public FunctionImplemented GetLaunchQueryParamSupport => FunctionImplemented.NotImplemented;

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
