// TODO consider removing this from the ISteamApps interface as it's entirely alien from ISteamApps002+

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
    /// Contains ISteamApps001 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMAPPS_INTERFACE_VERSION001")]
    public class ISteamApps001 : SteamInterfaceWrapper, ISteamApps
    {
        public ISteamApps001(IntPtr interfacePtr) : base(interfacePtr) { }

        // STEAMAPPS_INTERFACE_VERSION001: int GetAppData( AppId_t nAppID, const char *pchKey, char *pchValue, int cchValueMax )
        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetAppDataDelegate(IntPtr thisPtr, UInt32 nAppID, IntPtr pchKey, StringBuilder pchValue, int cchValueMax);
        public int GetAppData(UInt32 nAppId, string pchKey, StringBuilder pchValue, int cchValueMax)
        {
            IntPtr keyUtf8 = Marshal.StringToCoTaskMemUTF8(pchKey);
            try
            {
                return GetDelegate<GetAppDataDelegate>()(InterfacePtr, nAppId, keyUtf8, pchValue, cchValueMax);
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUTF8(keyUtf8);
            }
        }
        public FunctionImplemented GetAppDataSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool BIsSubscribed() { throw new NotImplementedException(); }
        public FunctionImplemented BIsSubscribedSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BIsLowViolence() { throw new NotImplementedException(); }
        public FunctionImplemented BIsLowViolenceSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BIsCyberCafe() { throw new NotImplementedException(); }
        public FunctionImplemented BIsCyberCafeSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BIsVACBanned() { throw new NotImplementedException(); }
        public FunctionImplemented BIsVACBannedSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public string GetCurrentGameLanguage() { throw new NotImplementedException(); }
        public FunctionImplemented GetCurrentGameLanguageSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public string GetAvailableGameLanguages() { throw new NotImplementedException(); }
        public FunctionImplemented GetAvailableGameLanguagesSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BIsSubscribedApp(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented BIsSubscribedAppSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public int BIsDlcInstalled(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented BIsDlcInstalledSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetEarliestPurchaseUnixTime(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented GetEarliestPurchaseUnixTimeSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BIsSubscribedFromFreeWeekend() { throw new NotImplementedException(); }
        public FunctionImplemented BIsSubscribedFromFreeWeekendSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public int GetDLCCount() { throw new NotImplementedException(); }
        public FunctionImplemented GetDLCCountSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BGetDLCDataByIndex(Int32 nDLCIndex, ref UInt32 pnAppID, StringBuilder pchName, Int32 cchNameBufferSize, ref bool pbAvailable) { throw new NotImplementedException(); }
        public FunctionImplemented BGetDLCDataByIndexSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void InstallDLC(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented InstallDLCSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void UninstallDLC(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented UninstallDLCSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void RequestAppProofOfPurchaseKey(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented RequestAppProofOfPurchaseKeySupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public string GetCurrentBetaName() { throw new NotImplementedException(); }
        public FunctionImplemented GetCurrentBetaNameSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void MarkContentCorrupt(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented MarkContentCorruptSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void GetInstalledDepots(UInt32 nAppId, UInt64[] pvecDepots, ref Int32 pnCount) { throw new NotImplementedException(); }
        public FunctionImplemented GetInstalledDepotsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public string GetAppInstallDir(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented GetAppInstallDirSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BIsAppInstalled(UInt32 nAppId) { throw new NotImplementedException(); }
        public FunctionImplemented BIsAppInstalledSupport => FunctionImplemented.NotImplemented;

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
