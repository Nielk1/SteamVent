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
    public interface ISteamApps {
        bool BIsSubscribed();
        bool BIsLowViolence();
        bool BIsCyberCafe();
        bool BIsVACBanned();
        string GetCurrentGameLanguage();
        string GetAvailableGameLanguages();
        bool BIsSubscribedApp(UInt32 nAppId);
        int BIsDlcInstalled(UInt32 nAppId);
        UInt32 GetEarliestPurchaseUnixTime(UInt32 nAppId);
        bool BIsSubscribedFromFreeWeekend();
        int GetDLCCount();
        bool BGetDLCDataByIndex(Int32 nDLCIndex, ref UInt32 pnAppID, StringBuilder pchName, Int32 cchNameBufferSize, ref bool pbAvailable);
        void InstallDLC(UInt32 nAppId);
        void UninstallDLC(UInt32 nAppId);
        void RequestAppProofOfPurchaseKey(UInt32 nAppId);
        string GetCurrentBetaName();
        void MarkContentCorrupt(UInt32 nAppId);
        void GetInstalledDepots(UInt32 nAppId, UInt64[] pvecDepots, ref Int32 pnCount);
        string GetAppInstallDir(UInt32 nAppId);
        bool BIsAppInstalled(UInt32 nAppId);
        UInt64 GetAppOwner(UInt32 nAppId);
        string GetLaunchQueryParam(string pchKey);
        void GetDlcDownloadProgress(UInt32 nAppId, ref UInt64 pnBytesDownloaded, ref UInt64 pnBytesTotal);
        UInt32 GetAppBuildId(UInt32 nAppId);
        void RequestAllProofOfPurchaseKeys();
        UInt32 GetFileDetails(string pszFileName);
        string GetLaunchCommandLine();
        bool BIsSubscribedFromFamilySharing();
        bool BIsTimedTrial(ref UInt32 punSecondsAllowed, ref UInt32 punSecondsPlayed);
        bool SetDlcContext(UInt32 nAppId);
        Int32 GetNumBetas(ref Int32 pnAvailable, ref Int32 pnPrivate);
        bool GetBetaInfo(Int32 iBetaIndex, ref UInt32 punFlags, ref UInt32 punBuildID, StringBuilder pchBetaName, Int32 cchBetaName, StringBuilder pchDescription, Int32 cchDescription, ref UInt32 punLastUpdated);
        bool SetActiveBeta(string pchBetaName);
        void SetGamePerformanceSetting(EGamePerformanceSetting setting);
        void SetGameRenderResolution(UInt32 unWidth, UInt32 unHeight);
    }
}
