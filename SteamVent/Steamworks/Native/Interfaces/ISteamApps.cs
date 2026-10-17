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
    public interface ISteamApps
    {
        static readonly public Type[] Versions =
        {
                typeof(ISteamApps001),
                typeof(ISteamApps002),
                typeof(ISteamApps003),
                typeof(ISteamApps004),
                typeof(ISteamApps005),
                typeof(ISteamApps006),
                typeof(ISteamApps007),
                typeof(ISteamApps008),
        };

        // Legacy (STEAMAPPS_INTERFACE_VERSION001 only) — reads a value from the app's data file.
        // Absent from the vtable in 002 and later, so those versions carry it as an [Obsolete] stub.
        int GetAppData(UInt32 nAppId, string pchKey, StringBuilder pchValue, int cchValueMax);
        FunctionImplemented GetAppDataSupport { get; }
        bool BIsSubscribed();
        FunctionImplemented BIsSubscribedSupport { get; }
        bool BIsLowViolence();
        FunctionImplemented BIsLowViolenceSupport { get; }
        bool BIsCyberCafe();
        FunctionImplemented BIsCyberCafeSupport { get; }
        bool BIsVACBanned();
        FunctionImplemented BIsVACBannedSupport { get; }
        string GetCurrentGameLanguage();
        FunctionImplemented GetCurrentGameLanguageSupport { get; }
        string GetAvailableGameLanguages();
        FunctionImplemented GetAvailableGameLanguagesSupport { get; }
        bool BIsSubscribedApp(UInt32 nAppId);
        FunctionImplemented BIsSubscribedAppSupport { get; }
        int BIsDlcInstalled(UInt32 nAppId);
        FunctionImplemented BIsDlcInstalledSupport { get; }
        UInt32 GetEarliestPurchaseUnixTime(UInt32 nAppId);
        FunctionImplemented GetEarliestPurchaseUnixTimeSupport { get; }
        bool BIsSubscribedFromFreeWeekend();
        FunctionImplemented BIsSubscribedFromFreeWeekendSupport { get; }
        int GetDLCCount();
        FunctionImplemented GetDLCCountSupport { get; }
        bool BGetDLCDataByIndex(Int32 nDLCIndex, ref UInt32 pnAppID, StringBuilder pchName, Int32 cchNameBufferSize, ref bool pbAvailable);
        FunctionImplemented BGetDLCDataByIndexSupport { get; }
        void InstallDLC(UInt32 nAppId);
        FunctionImplemented InstallDLCSupport { get; }
        void UninstallDLC(UInt32 nAppId);
        FunctionImplemented UninstallDLCSupport { get; }
        void RequestAppProofOfPurchaseKey(UInt32 nAppId);
        FunctionImplemented RequestAppProofOfPurchaseKeySupport { get; }
        string GetCurrentBetaName();
        FunctionImplemented GetCurrentBetaNameSupport { get; }
        void MarkContentCorrupt(UInt32 nAppId);
        FunctionImplemented MarkContentCorruptSupport { get; }
        void GetInstalledDepots(UInt32 nAppId, UInt64[] pvecDepots, ref Int32 pnCount);
        FunctionImplemented GetInstalledDepotsSupport { get; }
        string GetAppInstallDir(UInt32 nAppId);
        FunctionImplemented GetAppInstallDirSupport { get; }
        bool BIsAppInstalled(UInt32 nAppId);
        FunctionImplemented BIsAppInstalledSupport { get; }
        UInt64 GetAppOwner(UInt32 nAppId);
        FunctionImplemented GetAppOwnerSupport { get; }
        string GetLaunchQueryParam(string pchKey);
        FunctionImplemented GetLaunchQueryParamSupport { get; }
        void GetDlcDownloadProgress(UInt32 nAppId, ref UInt64 pnBytesDownloaded, ref UInt64 pnBytesTotal);
        FunctionImplemented GetDlcDownloadProgressSupport { get; }
        UInt32 GetAppBuildId(UInt32 nAppId);
        FunctionImplemented GetAppBuildIdSupport { get; }
        void RequestAllProofOfPurchaseKeys();
        FunctionImplemented RequestAllProofOfPurchaseKeysSupport { get; }
        UInt32 GetFileDetails(string pszFileName);
        FunctionImplemented GetFileDetailsSupport { get; }
        string GetLaunchCommandLine();
        FunctionImplemented GetLaunchCommandLineSupport { get; }
        bool BIsSubscribedFromFamilySharing();
        FunctionImplemented BIsSubscribedFromFamilySharingSupport { get; }
        bool BIsTimedTrial(ref UInt32 punSecondsAllowed, ref UInt32 punSecondsPlayed);
        FunctionImplemented BIsTimedTrialSupport { get; }
        bool SetDlcContext(UInt32 nAppId);
        FunctionImplemented SetDlcContextSupport { get; }
        Int32 GetNumBetas(ref Int32 pnAvailable, ref Int32 pnPrivate);
        FunctionImplemented GetNumBetasSupport { get; }
        bool GetBetaInfo(Int32 iBetaIndex, ref UInt32 punFlags, ref UInt32 punBuildID, StringBuilder pchBetaName, Int32 cchBetaName, StringBuilder pchDescription, Int32 cchDescription, ref UInt32 punLastUpdated);
        FunctionImplemented GetBetaInfoSupport { get; }
        bool SetActiveBeta(string pchBetaName);
        FunctionImplemented SetActiveBetaSupport { get; }
        void SetGamePerformanceSetting(EGamePerformanceSetting setting);
        FunctionImplemented SetGamePerformanceSettingSupport { get; }
        void SetGameRenderResolution(UInt32 unWidth, UInt32 unHeight);
        FunctionImplemented SetGameRenderResolutionSupport { get; }
    }
}
