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

namespace SteamVent.InterProc.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamApps008 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMUGC_INTERFACE_VERSION005")]
    public class ISteamUGC005 : SteamInterfaceWrapper, ISteamUGC
    {
        public ISteamUGC005(IntPtr interfacePtr) : base(interfacePtr) { }

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

        #region VTableIndex(2)
        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateQueryUGCDetailsRequestDelegate(IntPtr thisPtr, ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        #endregion
        public UInt64 CreateQueryUGCDetailsRequest(ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs) =>
            GetDelegate<CreateQueryUGCDetailsRequestDelegate>()(InterfacePtr, ref pvecPublishedFileID, unNumPublishedFileIDs);

        #region VTableIndex(3)
        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SendQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        #endregion
        public UInt64 SendQueryUGCRequest(UGCQueryHandle_t handle) =>
            GetDelegate<SendQueryUGCRequestDelegate>()(InterfacePtr, handle);

        #region VTableIndex(4)
        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCResultDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails);
        #endregion
        public bool GetQueryUGCResult(UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails) =>
            GetDelegate<GetQueryUGCResultDelegate>()(InterfacePtr, handle, index, ref pDetails);

        #region VTableIndex(5)
        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCPreviewURLDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize);
        #endregion
        public bool GetQueryUGCPreviewURL(UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize) =>
            GetDelegate<GetQueryUGCPreviewURLDelegate>()(InterfacePtr, handle, index, ref pchURL, cchURLSize);

        #region VTableIndex(6)
        [VTableIndex(6), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCMetadataDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize);
        #endregion
        public bool GetQueryUGCMetadata(UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize) =>
            GetDelegate<GetQueryUGCMetadataDelegate>()(InterfacePtr, handle, index, ref pchMetadata, cchMetadatasize);

        #region VTableIndex(7)
        [VTableIndex(7), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCChildrenDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries);
        #endregion
        public bool GetQueryUGCChildren(UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries) =>
            GetDelegate<GetQueryUGCChildrenDelegate>()(InterfacePtr, handle, index, ref pvecPublishedFileID, cMaxEntries);

        #region VTableIndex(8)
        [VTableIndex(8), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCStatisticDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue);
        #endregion
        public bool GetQueryUGCStatistic(UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue) =>
            GetDelegate<GetQueryUGCStatisticDelegate>()(InterfacePtr, handle, index, eStatType, ref pStatValue);

        #region VTableIndex(9)
        [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetQueryUGCNumAdditionalPreviewsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index);
        #endregion
        public UInt32 GetQueryUGCNumAdditionalPreviews(UGCQueryHandle_t handle, UInt32 index) =>
            GetDelegate<GetQueryUGCNumAdditionalPreviewsDelegate>()(InterfacePtr, handle, index);

        #region VTableIndex(10)
        [VTableIndex(10), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCAdditionalPreviewDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref bool pbIsImage);
        #endregion
        public bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref bool pbIsImage) =>
            GetDelegate<GetQueryUGCAdditionalPreviewDelegate>()(InterfacePtr, handle, index, previewIndex, ref pchURLOrVideoID, cchURLSize, ref pbIsImage);

        #region VTableIndex(11)
        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool ReleaseQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        #endregion
        public bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle) =>
            GetDelegate<ReleaseQueryUGCRequestDelegate>()(InterfacePtr, handle);

        #region VTableIndex(12)
        [VTableIndex(12), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddRequiredTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        #endregion
        public bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName) =>
            GetDelegate<AddRequiredTagDelegate>()(InterfacePtr, handle, pTagName);

        #region VTableIndex(13)
        [VTableIndex(13), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddExcludedTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        #endregion
        public bool AddExcludedTag(UGCQueryHandle_t handle, string pTagName) =>
            GetDelegate<AddExcludedTagDelegate>()(InterfacePtr, handle, pTagName);

        #region VTableIndex(14)
        [VTableIndex(14), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnLongDescriptionDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnLongDescription);
        #endregion
        public bool SetReturnLongDescription(UGCQueryHandle_t handle, bool bReturnLongDescription) =>
            GetDelegate<SetReturnLongDescriptionDelegate>()(InterfacePtr, handle, bReturnLongDescription);

        #region VTableIndex(15)
        [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnMetadataDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnMetadata);
        #endregion
        public bool SetReturnMetadata( UGCQueryHandle_t handle, bool bReturnMetadata) =>
            GetDelegate<SetReturnMetadataDelegate>()(InterfacePtr, handle, bReturnMetadata);

        #region VTableIndex(16)
        [VTableIndex(16), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnChildrenDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnChildren);
        #endregion
        public bool SetReturnChildren( UGCQueryHandle_t handle, bool bReturnChildren) =>
            GetDelegate<SetReturnChildrenDelegate>()(InterfacePtr, handle, bReturnChildren);

        #region VTableIndex(17)
        [VTableIndex(17), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnAdditionalPreviewsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnAdditionalPreviews);
        #endregion
        public bool SetReturnAdditionalPreviews( UGCQueryHandle_t handle, bool bReturnAdditionalPreviews) =>
            GetDelegate<SetReturnAdditionalPreviewsDelegate>()(InterfacePtr, handle, bReturnAdditionalPreviews);

        #region VTableIndex(18)
        [VTableIndex(18), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnTotalOnlyDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnTotalOnly);
        #endregion
        public bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool bReturnTotalOnly) =>
            GetDelegate<SetReturnTotalOnlyDelegate>()(InterfacePtr, handle, bReturnTotalOnly);

        #region VTableIndex(19)
        [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetAllowCachedResponseDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds);
        #endregion
        public bool SetAllowCachedResponse(UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds) =>
            GetDelegate<SetAllowCachedResponseDelegate>()(InterfacePtr, handle, unMaxAgeSeconds);

        #region VTableIndex(20)
        [VTableIndex(20), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetCloudFileNameFilterDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pMatchCloudFileName);
        #endregion
        public bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName) =>
            GetDelegate<SetCloudFileNameFilterDelegate>()(InterfacePtr, handle, pMatchCloudFileName);

        #region VTableIndex(21)
        [VTableIndex(21), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetMatchAnyTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bMatchAnyTag);
        #endregion
        public bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag) =>
            GetDelegate<SetMatchAnyTagDelegate>()(InterfacePtr, handle, bMatchAnyTag);

        #region VTableIndex(22)
        [VTableIndex(22), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetSearchTextDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pSearchText);
        #endregion
        public bool SetSearchText(UGCQueryHandle_t handle, string pSearchText) =>
            GetDelegate<SetSearchTextDelegate>()(InterfacePtr, handle, pSearchText);

        #region VTableIndex(23)
        [VTableIndex(23), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetRankedByTrendDaysDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unDays);
        #endregion
        public bool SetRankedByTrendDays(UGCQueryHandle_t handle, UInt32 unDays) =>
            GetDelegate<SetRankedByTrendDaysDelegate>()(InterfacePtr, handle, unDays);

        [Obsolete("Not implemented in this version.", true)] public UInt64 RequestUGCDetails(UInt64 nPublishedFileID) { throw new NotImplementedException(); }

        #region VTableIndex(24)
        [VTableIndex(24), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RequestUGCDetailsDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds);
        #endregion
        [Obsolete("Use CreateQueryUGCDetailsRequest instead")]
        public UInt64 RequestUGCDetails(UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds) =>
            GetDelegate<RequestUGCDetailsDelegate>()(InterfacePtr, nPublishedFileID, unMaxAgeSeconds);


        #region VTableIndex(25)
        [VTableIndex(25), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateItemDelegate(IntPtr thisPtr, UInt32 nConsumerAppId, EWorkshopFileType eFileType);
        #endregion
        public UInt64 CreateItem(UInt32 nConsumerAppId, EWorkshopFileType eFileType) =>
            GetDelegate<CreateItemDelegate>()(InterfacePtr, nConsumerAppId, eFileType);

        #region VTableIndex(26)
        [VTableIndex(26), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StartItemUpdateDelegate(IntPtr thisPtr, UInt32 nConsumerAppId, UInt64 nPublishedFileID);
        #endregion
        public UInt64 StartItemUpdate(UInt32 nConsumerAppId, UInt64 nPublishedFileID) =>
            GetDelegate<StartItemUpdateDelegate>()(InterfacePtr, nConsumerAppId, nPublishedFileID);

        #region VTableIndex(27)
        [VTableIndex(27), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemTitleDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchTitle);
        #endregion
        public bool SetItemTitle(UInt64 handle, string pchTitle) =>
            GetDelegate<SetItemTitleDelegate>()(InterfacePtr, handle, pchTitle);

        #region VTableIndex(28)
        [VTableIndex(28), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemDescriptionDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchDescription);
        #endregion
        public bool SetItemDescription(UInt64 handle, string pchDescription) =>
            GetDelegate<SetItemDescriptionDelegate>()(InterfacePtr, handle, pchDescription);


        #region VTableIndex(29)
        [VTableIndex(29), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemMetadataDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchMetaData);
        #endregion
        public bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData) =>
            GetDelegate<SetItemMetadataDelegate>()(InterfacePtr, handle, pchMetaData);


        #region VTableIndex(30)
        [VTableIndex(30), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemVisibilityDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility);
        #endregion
        public bool SetItemVisibility(UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility) =>
            GetDelegate<SetItemVisibilityDelegate>()(InterfacePtr, handle, eVisibility);

        #region VTableIndex(31)
        [VTableIndex(31), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemTagsDelegate(IntPtr thisPtr, UGCUpdateHandle_t updateHandle, ref SteamParamStringArray_t pTags);
        #endregion
        public bool SetItemTags(UGCUpdateHandle_t updateHandle, ref SteamParamStringArray_t pTags) =>
            GetDelegate<SetItemTagsDelegate>()(InterfacePtr, updateHandle, ref pTags);

        #region VTableIndex(32)
        [VTableIndex(32), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemContentDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszContentFolder);
        #endregion
        public bool SetItemContent(UGCUpdateHandle_t handle, string pszContentFolder) =>
            GetDelegate<SetItemContentDelegate>()(InterfacePtr, handle, pszContentFolder);

        #region VTableIndex(33)
        [VTableIndex(33), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemPreviewDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszPreviewFile);
        #endregion
        public bool SetItemPreview(UGCUpdateHandle_t handle, string pszPreviewFile) =>
            GetDelegate<SetItemPreviewDelegate>()(InterfacePtr, handle, pszPreviewFile);

        #region VTableIndex(34)
        [VTableIndex(34), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SubmitItemUpdateDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchChangeNote);
        #endregion
        public UInt64 SubmitItemUpdate(UGCUpdateHandle_t handle, string pchChangeNote) =>
            GetDelegate<SubmitItemUpdateDelegate>()(InterfacePtr, handle, pchChangeNote);

        #region VTableIndex(35)
        [VTableIndex(35), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate EItemUpdateStatus GetItemUpdateProgressDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal);
        #endregion
        public EItemUpdateStatus GetItemUpdateProgress(UGCUpdateHandle_t handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal) =>
            GetDelegate<GetItemUpdateProgressDelegate>()(InterfacePtr, handle, ref punBytesProcessed, ref punBytesTotal);

        // AddItemToFavorites
        // RemoteItemFromFavorites

        #region VTableIndex(38)
        [VTableIndex(38), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SubscribeItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        #endregion
        public UInt64 SubscribeItem(UInt64 nPublishedFileID) =>
            GetDelegate<SubscribeItemDelegate>()(InterfacePtr, nPublishedFileID);

        #region VTableIndex(39)
        [VTableIndex(39), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 UnsubscribeItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        #endregion
        public UInt64 UnsubscribeItem(UInt64 nPublishedFileID) =>
            GetDelegate<UnsubscribeItemDelegate>()(InterfacePtr, nPublishedFileID);

        #region VTableIndex(40)
        [VTableIndex(40), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetNumSubscribedItemsDelegate(IntPtr thisPtr);
        #endregion
        public UInt32 GetNumSubscribedItems() =>
            GetDelegate<GetNumSubscribedItemsDelegate>()(InterfacePtr);

        #region VTableIndex(41)
        [VTableIndex(41), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetSubscribedItemsDelegate(IntPtr thisPtr, ref UInt64 pvecPublishedFileID, UInt32 cMaxEntries);
        #endregion
        public UInt32 GetSubscribedItems(ref UInt64 pvecPublishedFileID, UInt32 cMaxEntries) =>
            GetDelegate<GetSubscribedItemsDelegate>()(InterfacePtr, ref pvecPublishedFileID, cMaxEntries);

        //GetItemState

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize) { throw new NotImplementedException(); }
        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref bool pbLegacyItem) { throw new NotImplementedException(); }

        #region VTableIndex(43)
        [VTableIndex(43), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetItemInstallInfoDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp);
        #endregion
        public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp) =>
            GetDelegate<GetItemInstallInfoDelegate>()(InterfacePtr, nPublishedFileID, ref punSizeOnDisk, pchFolder, cchFolderSize, ref punTimeStamp);

        [Obsolete("Not implemented in this version.", true)] public bool GetItemUpdateInfo(UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) { throw new NotImplementedException(); }

        #region VTableIndex(44)
        [VTableIndex(44), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetItemDownloadInfoDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);
        #endregion
        public bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) =>
            GetDelegate<GetItemDownloadInfoDelegate>()(InterfacePtr, nPublishedFileID, ref punBytesDownloaded, ref punBytesTotal);

        //DownloadItem
    }
}
