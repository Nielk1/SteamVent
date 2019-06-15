using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.InterProc.Attributes;
using SteamVent.InterProc.Interop;

namespace SteamVent.InterProc.Interfaces
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
        //BGetDLCDataByIndex
        void InstallDLC(UInt32 nAppId);
        void UninstallDLC(UInt32 nAppId);
        //RequestAppProofOfPurchaseKey
        //GetCurrentBetaName
        //MarkContentCorrupt
        //GetInstalledDepots
        //GetAppInstallDir
        bool BIsAppInstalled(UInt32 nAppId);
        //GetAppOwner
        //GetLaunchQueryParam
        //GetDlcDownloadProgress
        //GetAppBuildId
        //RequestAllProofOfPurchaseKeys
        //GetFileDetails
        //GetFileDetails
        //BIsSubscribedFromFamilySharing
    }
}
