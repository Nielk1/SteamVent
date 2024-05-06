using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.InterProc.Attributes;
using SteamVent.InterProc.Interop;
using UGCQueryHandle_t = System.UInt64;
using PublishedFileId_t = System.UInt64;
using UGCUpdateHandle_t = System.UInt64;
using SteamVent.Common.InterProc;

namespace SteamVent.InterProc.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamApps008 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMUGC_INTERFACE_VERSION001")]
    public class ISteamUGC001 : SteamInterfaceWrapper, ISteamUGC
    {
        public ISteamUGC001(IntPtr interfacePtr) : base(interfacePtr) { }

        #region VTableIndex(0)
        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateQueryUserUGCRequestDelegate(IntPtr thisPtr, UInt32 unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
        #endregion
        public UInt64 CreateQueryUserUGCRequest(UInt32 unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage) =>
            GetDelegate<CreateQueryUserUGCRequestDelegate>()(InterfacePtr, unAccountID, eListType, eMatchingUGCType, eSortOrder, nCreatorAppID, nConsumerAppID, unPage);

        #region VTableIndex(1)
        [VTableIndex(1), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateQueryAllUGCRequestDelegate(IntPtr thisPtr, EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
        #endregion
        public UInt64 CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage) =>
            GetDelegate<CreateQueryAllUGCRequestDelegate>()(InterfacePtr, eQueryType, eMatchingeMatchingUGCTypeFileType, nCreatorAppID, nConsumerAppID, unPage);

        [Obsolete("Not implemented in this version.", true)] public UInt64 CreateQueryUGCDetailsRequest(ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs) { throw new NotImplementedException(); }

        #region VTableIndex(2)
        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SendQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        #endregion
        public UInt64 SendQueryUGCRequest(UGCQueryHandle_t handle) =>
            GetDelegate<SendQueryUGCRequestDelegate>()(InterfacePtr, handle);

        #region VTableIndex(3)
        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCResultDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails);
        #endregion
        public bool GetQueryUGCResult(UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails) =>
            GetDelegate<GetQueryUGCResultDelegate>()(InterfacePtr, handle, index, ref pDetails);

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCPreviewURL(UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCMetadata(UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCChildren(UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCStatistic(UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public UInt32 GetQueryUGCNumAdditionalPreviews(UGCQueryHandle_t handle, UInt32 index) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref bool pbIsImage) { throw new NotImplementedException(); }

        #region VTableIndex(4)
        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool ReleaseQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        #endregion
        public bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle) =>
            GetDelegate<ReleaseQueryUGCRequestDelegate>()(InterfacePtr, handle);

        #region VTableIndex(5)
        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddRequiredTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        #endregion
        public bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName) =>
            GetDelegate<AddRequiredTagDelegate>()(InterfacePtr, handle, pTagName);

        #region VTableIndex(6)
        [VTableIndex(6), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddExcludedTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        #endregion
        public bool AddExcludedTag(UGCQueryHandle_t handle, string pTagName) =>
            GetDelegate<AddExcludedTagDelegate>()(InterfacePtr, handle, pTagName);

        #region VTableIndex(7)
        [VTableIndex(7), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnLongDescriptionDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnLongDescription);
        #endregion
        public bool SetReturnLongDescription(UGCQueryHandle_t handle, bool bReturnLongDescription) =>
            GetDelegate<SetReturnLongDescriptionDelegate>()(InterfacePtr, handle, bReturnLongDescription);

        [Obsolete("Not implemented in this version.", true)] public bool SetReturnMetadata(UGCQueryHandle_t handle, bool bReturnMetadata) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool SetReturnChildren(UGCQueryHandle_t handle, bool bReturnChildren) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool SetReturnAdditionalPreviews(UGCQueryHandle_t handle, bool bReturnAdditionalPreviews) { throw new NotImplementedException(); }

        #region VTableIndex(8)
        [VTableIndex(8), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnTotalOnlyDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnTotalOnly);
        #endregion
        public bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool bReturnTotalOnly) =>
            GetDelegate<SetReturnTotalOnlyDelegate>()(InterfacePtr, handle, bReturnTotalOnly);

        #region VTableIndex(9)
        [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetCloudFileNameFilterDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pMatchCloudFileName);
        #endregion
        public bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName) =>
            GetDelegate<SetCloudFileNameFilterDelegate>()(InterfacePtr, handle, pMatchCloudFileName);

        #region VTableIndex(10)
        [VTableIndex(10), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetMatchAnyTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bMatchAnyTag);
        #endregion
        public bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag) =>
            GetDelegate<SetMatchAnyTagDelegate>()(InterfacePtr, handle, bMatchAnyTag);

        #region VTableIndex(11)
        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetSearchTextDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pSearchText);
        #endregion
        public bool SetSearchText(UGCQueryHandle_t handle, string pSearchText) =>
            GetDelegate<SetSearchTextDelegate>()(InterfacePtr, handle, pSearchText);

        #region VTableIndex(12)
        [VTableIndex(12), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetRankedByTrendDaysDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unDays);
        #endregion
        public bool SetRankedByTrendDays(UGCQueryHandle_t handle, UInt32 unDays) =>
            GetDelegate<SetRankedByTrendDaysDelegate>()(InterfacePtr, handle, unDays);

        #region VTableIndex(13)
        [VTableIndex(13), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RequestUGCDetailsDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        #endregion
        public UInt64 RequestUGCDetails(UInt64 nPublishedFileID) =>
            GetDelegate<RequestUGCDetailsDelegate>()(InterfacePtr, nPublishedFileID);

        [Obsolete("Not implemented in this version.", true)] public UInt64 RequestUGCDetails(UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public UInt64 CreateItem(UInt32 nConsumerAppId, EWorkshopFileType eFileType) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public UInt64 StartItemUpdate(UInt32 nConsumerAppId, UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool SetItemTitle(UInt64 handle, string pchTitle) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool SetItemDescription(UInt64 handle, string pchDescription) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool SetItemVisibility(UInt64 handle, ERemoteStoragePublishedFileVisibility eVisibility) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool SetItemTags(UInt64 updateHandle, ref SteamParamStringArray_t pTags) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool SetItemContent(UInt64 handle, string pszContentFolder) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool SetItemPreview(UInt64 handle, string pszPreviewFile) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public UInt64 SubmitItemUpdate(UInt64 handle, string pchChangeNote) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public EItemUpdateStatus GetItemUpdateProgress(UInt64 handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public UInt64 SubscribeItem(UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public UInt64 UnsubscribeItem(UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public UInt32 GetNumSubscribedItems() { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public UInt32 GetSubscribedItems(UInt64[] pvecPublishedFileID, UInt32 cMaxEntries) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public EItemState GetItemState(UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref bool pbLegacyItem) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetItemUpdateInfo(UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) { throw new NotImplementedException(); }

    }
}
