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
    /// Contains ISteamApps009 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMAPPS_INTERFACE_VERSION009")]
    public class ISteamApps009 : SteamInterfaceWrapper, ISteamApps
    {
        public ISteamApps009(IntPtr interfacePtr) : base(interfacePtr) { }

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

        #region VTableIndex(7)
        [VTableIndex(7), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int BIsDlcInstalledDelegate(IntPtr thisPtr, UInt32 nAppId);
        #endregion
        public int BIsDlcInstalled(UInt32 nAppId) =>
            GetDelegate<BIsDlcInstalledDelegate>()(InterfacePtr, nAppId);

        #region VTableIndex(8)
        [VTableIndex(8), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetEarliestPurchaseUnixTimeDelegate(IntPtr thisPtr, UInt32 nAppId);
        #endregion
        public UInt32 GetEarliestPurchaseUnixTime(UInt32 nAppId) =>
            GetDelegate<GetEarliestPurchaseUnixTimeDelegate>()(InterfacePtr, nAppId);

        #region VTableIndex(9)
        [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsSubscribedFromFreeWeekendDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsSubscribedFromFreeWeekend() =>
            GetDelegate<BIsSubscribedFromFreeWeekendDelegate>()(InterfacePtr);

        #region VTableIndex(10)
        [VTableIndex(10), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetDLCCountDelegate(IntPtr thisPtr);
        #endregion
        public int GetDLCCount() =>
            GetDelegate<GetDLCCountDelegate>()(InterfacePtr);

        #region VTableIndex(11)
        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BGetDLCDataByIndexDelegate(IntPtr thisPtr, Int32 iDLC, ref UInt32 pAppID, ref bool pbAvailable, StringBuilder pchName, Int32 cchNameBufferSize);
        #endregion
        public bool BGetDLCDataByIndex(Int32 nDLCIndex, ref UInt32 pnAppID, StringBuilder pchName, Int32 cchNameBufferSize, ref bool pbAvailable) =>
            GetDelegate<BGetDLCDataByIndexDelegate>()(InterfacePtr, nDLCIndex, ref pnAppID, ref pbAvailable, pchName, cchNameBufferSize);

        #region VTableIndex(12)
        [VTableIndex(12), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InstallDLCDelegate(IntPtr thisPtr, UInt32 nAppId);
        #endregion
        public void InstallDLC(UInt32 nAppId) =>
            GetDelegate<InstallDLCDelegate>()(InterfacePtr, nAppId);

        #region VTableIndex(13)
        [VTableIndex(13), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void UninstallDLCDelegate(IntPtr thisPtr, UInt32 nAppId);
        #endregion
        public void UninstallDLC(UInt32 nAppId) =>
            GetDelegate<UninstallDLCDelegate>()(InterfacePtr, nAppId);

        #region VTableIndex(14)
        [VTableIndex(14), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void RequestAppProofOfPurchaseKeyDelegate(IntPtr thisPtr, UInt32 nAppID);
        #endregion
        public void RequestAppProofOfPurchaseKey(UInt32 nAppId) =>
            GetDelegate<RequestAppProofOfPurchaseKeyDelegate>()(InterfacePtr, nAppId);

        #region VTableIndex(15)
        [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetCurrentBetaNameDelegate(IntPtr thisPtr, IntPtr pchName, Int32 cchNameBufferSize);
        #endregion
        public string GetCurrentBetaName()
        {
            // Official STEAMAPPS_INTERFACE_VERSION008: bool GetCurrentBetaName( char *pchName, int cchNameBufferSize )
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

        #region VTableIndex(16)
        [VTableIndex(16), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void MarkContentCorruptDelegate(IntPtr thisPtr, UInt32 nAppID);
        #endregion
        public void MarkContentCorrupt(UInt32 nAppId) =>
            GetDelegate<MarkContentCorruptDelegate>()(InterfacePtr, nAppId);

        #region VTableIndex(17)
        [VTableIndex(17), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void GetInstalledDepotsDelegate(IntPtr thisPtr, UInt32 nAppID, UInt64[] pvecDepots, ref Int32 pnCount);
        #endregion
        public void GetInstalledDepots(UInt32 nAppId, UInt64[] pvecDepots, ref Int32 pnCount) =>
            GetDelegate<GetInstalledDepotsDelegate>()(InterfacePtr, nAppId, pvecDepots, ref pnCount);

        #region VTableIndex(18)
        [VTableIndex(18), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetAppInstallDirDelegate(IntPtr thisPtr, UInt32 nAppId, IntPtr pchFolder, UInt32 cchFolderBufferSize);
        #endregion
        public string GetAppInstallDir(UInt32 nAppId)
        {
            const UInt32 size = 1024;
            byte[] _pchFolder = new byte[size];
            IntPtr pchFolder = Marshal.AllocHGlobal(_pchFolder.Length + 1);
            try
            {
                UInt32 read = GetDelegate<GetAppInstallDirDelegate>()(InterfacePtr, nAppId, pchFolder, size);
                Marshal.Copy(pchFolder, _pchFolder, 0, (int)read);
                return Encoding.UTF8.GetString(_pchFolder, 0, (int)read);
            }
            finally
            {
                Marshal.FreeHGlobal(pchFolder);
            }
        }

        #region VTableIndex(19)
        [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetIsAppInstalledDelegate(IntPtr thisPtr, UInt32 nAppId);
        #endregion
        public bool BIsAppInstalled(UInt32 nAppId) =>
            GetDelegate<GetIsAppInstalledDelegate>()(InterfacePtr, nAppId);

        #region VTableIndex(20)
        [VTableIndex(20), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 GetAppOwnerDelegate(IntPtr thisPtr);
        #endregion
        public UInt64 GetAppOwner(UInt32 nAppId) =>
            GetDelegate<GetAppOwnerDelegate>()(InterfacePtr);

        #region VTableIndex(21)
        [VTableIndex(21), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetLaunchQueryParamDelegate(IntPtr thisPtr, IntPtr pchKey);
        #endregion
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

        #region VTableIndex(22)
        [VTableIndex(22), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void GetDlcDownloadProgressDelegate(IntPtr thisPtr, UInt32 nAppID, ref UInt64 pnBytesDownloaded, ref UInt64 pnBytesTotal);
        #endregion
        public void GetDlcDownloadProgress(UInt32 nAppId, ref UInt64 pnBytesDownloaded, ref UInt64 pnBytesTotal) =>
            GetDelegate<GetDlcDownloadProgressDelegate>()(InterfacePtr, nAppId, ref pnBytesDownloaded, ref pnBytesTotal);

        #region VTableIndex(23)
        [VTableIndex(23), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetAppBuildIdDelegate(IntPtr thisPtr);
        #endregion
        public UInt32 GetAppBuildId(UInt32 nAppId) =>
            GetDelegate<GetAppBuildIdDelegate>()(InterfacePtr);

        #region VTableIndex(24)
        [VTableIndex(24), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void RequestAllProofOfPurchaseKeysDelegate(IntPtr thisPtr);
        #endregion
        public void RequestAllProofOfPurchaseKeys() =>
            GetDelegate<RequestAllProofOfPurchaseKeysDelegate>()(InterfacePtr);

        #region VTableIndex(25)
        [VTableIndex(25), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetFileDetailsDelegate(IntPtr thisPtr, IntPtr pszFileName);
        #endregion
        public UInt32 GetFileDetails(string pszFileName)
        {
            IntPtr fileUtf8 = Marshal.StringToCoTaskMemUTF8(pszFileName);
            try
            {
                return GetDelegate<GetFileDetailsDelegate>()(InterfacePtr, fileUtf8);
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUTF8(fileUtf8);
            }
        }

        #region VTableIndex(26)
        [VTableIndex(26), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetLaunchCommandLineDelegate(IntPtr thisPtr, IntPtr pszCommandLine, int cubCommandLine);
        #endregion
        public string GetLaunchCommandLine()
        {
            int size = 1024;
            byte[] buffer = new byte[size];
            IntPtr pch = Marshal.AllocHGlobal(size + 1);
            try
            {
                int written = GetDelegate<GetLaunchCommandLineDelegate>()(InterfacePtr, pch, size);
                int copy = written > 0 ? Math.Min(written, size) : 0;
                Marshal.Copy(pch, buffer, 0, copy);
                return Encoding.UTF8.GetString(buffer, 0, copy);
            }
            finally
            {
                Marshal.FreeHGlobal(pch);
            }
        }

        #region VTableIndex(27)
        [VTableIndex(27), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsSubscribedFromFamilySharingDelegate(IntPtr thisPtr);
        #endregion
        public bool BIsSubscribedFromFamilySharing() =>
            GetDelegate<BIsSubscribedFromFamilySharingDelegate>()(InterfacePtr);

        #region VTableIndex(28)
        [VTableIndex(28), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsTimedTrialDelegate(IntPtr thisPtr, ref UInt32 punSecondsAllowed, ref UInt32 punSecondsPlayed);
        #endregion
        public bool BIsTimedTrial(ref UInt32 punSecondsAllowed, ref UInt32 punSecondsPlayed) =>
            GetDelegate<BIsTimedTrialDelegate>()(InterfacePtr, ref punSecondsAllowed, ref punSecondsPlayed);

        #region VTableIndex(29)
        [VTableIndex(29), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetDlcContextDelegate(IntPtr thisPtr, UInt32 nAppID);
        #endregion
        public bool SetDlcContext(UInt32 nAppId) =>
            GetDelegate<SetDlcContextDelegate>()(InterfacePtr, nAppId);

        [Obsolete("Not implemented in this version.", true)] public int GetAppData(UInt32 nAppId, string pchKey, StringBuilder pchValue, int cchValueMax) { throw new NotImplementedException(); }

        #region VTableIndex(30)
        [VTableIndex(30), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Int32 GetNumBetasDelegate(IntPtr thisPtr, ref Int32 pnAvailable, ref Int32 pnPrivate);
        #endregion
        public Int32 GetNumBetas(ref Int32 pnAvailable, ref Int32 pnPrivate) =>
            GetDelegate<GetNumBetasDelegate>()(InterfacePtr, ref pnAvailable, ref pnPrivate);

        #region VTableIndex(31)
        [VTableIndex(31), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetBetaInfoDelegate(IntPtr thisPtr, Int32 iBetaIndex, ref UInt32 punFlags, ref UInt32 punBuildID, StringBuilder pchBetaName, Int32 cchBetaName, StringBuilder pchDescription, Int32 cchDescription, ref UInt32 punLastUpdated);
        #endregion
        public bool GetBetaInfo(Int32 iBetaIndex, ref UInt32 punFlags, ref UInt32 punBuildID, StringBuilder pchBetaName, Int32 cchBetaName, StringBuilder pchDescription, Int32 cchDescription, ref UInt32 punLastUpdated) =>
            GetDelegate<GetBetaInfoDelegate>()(InterfacePtr, iBetaIndex, ref punFlags, ref punBuildID, pchBetaName, cchBetaName, pchDescription, cchDescription, ref punLastUpdated);

        #region VTableIndex(32)
        [VTableIndex(32), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetActiveBetaDelegate(IntPtr thisPtr, IntPtr pchBetaName);
        #endregion
        public bool SetActiveBeta(string pchBetaName)
        {
            IntPtr betaUtf8 = Marshal.StringToCoTaskMemUTF8(pchBetaName);
            try
            {
                return GetDelegate<SetActiveBetaDelegate>()(InterfacePtr, betaUtf8);
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUTF8(betaUtf8);
            }
        }

        #region VTableIndex(33)
        [VTableIndex(33), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetGamePerformanceSettingDelegate(IntPtr thisPtr, EGamePerformanceSetting setting);
        #endregion
        public void SetGamePerformanceSetting(EGamePerformanceSetting setting) =>
            GetDelegate<SetGamePerformanceSettingDelegate>()(InterfacePtr, setting);

        #region VTableIndex(34)
        [VTableIndex(34), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetGameRenderResolutionDelegate(IntPtr thisPtr, UInt32 unWidth, UInt32 unHeight);
        #endregion
        public void SetGameRenderResolution(UInt32 unWidth, UInt32 unHeight) =>
            GetDelegate<SetGameRenderResolutionDelegate>()(InterfacePtr, unWidth, unHeight);
    }
}
