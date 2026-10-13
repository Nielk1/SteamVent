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

        [Obsolete("Not implemented in this version.", true)] public bool BIsSubscribed() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool BIsLowViolence() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool BIsCyberCafe() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool BIsVACBanned() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public string GetCurrentGameLanguage() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public string GetAvailableGameLanguages() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool BIsSubscribedApp(UInt32 nAppId) { throw new NotImplementedException(); }

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
