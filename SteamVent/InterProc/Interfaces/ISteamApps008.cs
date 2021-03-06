﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.InterProc.Attributes;
using SteamVent.InterProc.Interop;

namespace SteamVent.InterProc.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamApps008 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMAPPS_INTERFACE_VERSION008")]
    public class ISteamApps008 : SteamInterfaceWrapper, ISteamApps
    {
        public ISteamApps008(IntPtr interfacePtr) : base(interfacePtr) { }

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

        //BGetDLCDataByIndex

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

        //RequestAppProofOfPurchaseKey
        //GetCurrentBetaName
        //MarkContentCorrupt
        //GetInstalledDepots

        #region VTableIndex(18)
        [VTableIndex(18), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate UInt32 GetAppInstallDirDelegate(IntPtr thisPtr, UInt32 nAppId, IntPtr pchFolder, UInt32 cchFolderBufferSize);
        #endregion
        public string GetAppInstallDir(UInt32 nAppId)
        {
            UInt32 size = 1024;
            byte[] _pchFolder = new byte[size];
            IntPtr pchFolder = Marshal.AllocHGlobal(_pchFolder.Length + 1);
            UInt32 read = GetDelegate<GetAppInstallDirDelegate>()(InterfacePtr, nAppId, pchFolder, size);
            return Encoding.UTF8.GetString(_pchFolder, 0, (int)read);
        }

        #region VTableIndex(19)
        [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetIsAppInstalledDelegate(IntPtr thisPtr, UInt32 nAppId);
        #endregion
        public bool BIsAppInstalled(UInt32 nAppId) =>
            GetDelegate<GetIsAppInstalledDelegate>()(InterfacePtr, nAppId);

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
