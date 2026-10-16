using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.Steamworks.Native.Attributes;
using SteamVent.Steamworks.Native;
using UGCQueryHandle_t = System.UInt64;
using PublishedFileId_t = System.UInt64;
using UGCUpdateHandle_t = System.UInt64;
using SteamVent.Core.Steamworks;

namespace SteamVent.Steamworks.Native.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamUGC014 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMUGC_INTERFACE_VERSION014")]
    public class ISteamUGC014 : SteamInterfaceWrapper, ISteamUGC
    {
        public ISteamUGC014(IntPtr interfacePtr) : base(interfacePtr) { }

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
        // STEAMUGC_INTERFACE_VERSION012+: deep-paging overload; pchCursor is null or "*" for the first page.
        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateQueryAllUGCRequestCursorDelegate(IntPtr thisPtr, EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, IntPtr pchCursor);
        #endregion
        public UInt64 CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, string pchCursor)
        {
            IntPtr cursorUtf8 = pchCursor == null ? IntPtr.Zero : Marshal.StringToCoTaskMemUTF8(pchCursor);
            try
            {
                return GetDelegate<CreateQueryAllUGCRequestCursorDelegate>()(InterfacePtr, eQueryType, eMatchingeMatchingUGCTypeFileType, nCreatorAppID, nConsumerAppID, cursorUtf8);
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUTF8(cursorUtf8);
            }
        }

        #region VTableIndex(3)
        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateQueryUGCDetailsRequestDelegate(IntPtr thisPtr, ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        #endregion
        public UInt64 CreateQueryUGCDetailsRequest(ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs) =>
            GetDelegate<CreateQueryUGCDetailsRequestDelegate>()(InterfacePtr, ref pvecPublishedFileID, unNumPublishedFileIDs);

        #region VTableIndex(4)
        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SendQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        #endregion
        public UInt64 SendQueryUGCRequest(UGCQueryHandle_t handle) =>
            GetDelegate<SendQueryUGCRequestDelegate>()(InterfacePtr, handle);

        #region VTableIndex(5)
        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCResultDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails);
        #endregion
        public bool GetQueryUGCResult(UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails) =>
            GetDelegate<GetQueryUGCResultDelegate>()(InterfacePtr, handle, index, ref pDetails);

        #region VTableIndex(6)
        [VTableIndex(6), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCPreviewURLDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize);
        #endregion
        public bool GetQueryUGCPreviewURL(UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize) =>
            GetDelegate<GetQueryUGCPreviewURLDelegate>()(InterfacePtr, handle, index, ref pchURL, cchURLSize);

        #region VTableIndex(7)
        [VTableIndex(7), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCMetadataDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize);
        #endregion
        public bool GetQueryUGCMetadata(UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize) =>
            GetDelegate<GetQueryUGCMetadataDelegate>()(InterfacePtr, handle, index, ref pchMetadata, cchMetadatasize);

        #region VTableIndex(8)
        [VTableIndex(8), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCChildrenDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries);
        #endregion
        public bool GetQueryUGCChildren(UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries) =>
            GetDelegate<GetQueryUGCChildrenDelegate>()(InterfacePtr, handle, index, ref pvecPublishedFileID, cMaxEntries);

        #region VTableIndex(9)
        [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCStatisticDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue);
        #endregion
        public bool GetQueryUGCStatistic(UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue) =>
            GetDelegate<GetQueryUGCStatisticDelegate>()(InterfacePtr, handle, index, eStatType, ref pStatValue);

        #region VTableIndex(10)
        [VTableIndex(10), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetQueryUGCNumAdditionalPreviewsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index);
        #endregion
        public UInt32 GetQueryUGCNumAdditionalPreviews(UGCQueryHandle_t handle, UInt32 index) =>
            GetDelegate<GetQueryUGCNumAdditionalPreviewsDelegate>()(InterfacePtr, handle, index);

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref bool pbIsImage) { throw new NotImplementedException(); }

        #region VTableIndex(11)
        // STEAMUGC_INTERFACE_VERSION008+: original file name + preview type replace the bool isImage flag.
        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCAdditionalPreviewFileDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref string pchOriginalFileName, UInt32 cchOriginalFileNameSize, ref EItemPreviewType pPreviewType);
        #endregion
        public bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref string pchOriginalFileName, UInt32 cchOriginalFileNameSize, ref EItemPreviewType pPreviewType) =>
            GetDelegate<GetQueryUGCAdditionalPreviewFileDelegate>()(InterfacePtr, handle, index, previewIndex, ref pchURLOrVideoID, cchURLSize, ref pchOriginalFileName, cchOriginalFileNameSize, ref pPreviewType);

        #region VTableIndex(15)
        [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool ReleaseQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        #endregion
        public bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle) =>
            GetDelegate<ReleaseQueryUGCRequestDelegate>()(InterfacePtr, handle);

        #region VTableIndex(16)
        [VTableIndex(16), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddRequiredTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        #endregion
        public bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName) =>
            GetDelegate<AddRequiredTagDelegate>()(InterfacePtr, handle, pTagName);

        #region VTableIndex(18)
        [VTableIndex(18), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddExcludedTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        #endregion
        public bool AddExcludedTag(UGCQueryHandle_t handle, string pTagName) =>
            GetDelegate<AddExcludedTagDelegate>()(InterfacePtr, handle, pTagName);

        #region VTableIndex(21)
        [VTableIndex(21), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnLongDescriptionDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnLongDescription);
        #endregion
        public bool SetReturnLongDescription(UGCQueryHandle_t handle, bool bReturnLongDescription) =>
            GetDelegate<SetReturnLongDescriptionDelegate>()(InterfacePtr, handle, bReturnLongDescription);

        #region VTableIndex(22)
        [VTableIndex(22), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnMetadataDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnMetadata);
        #endregion
        public bool SetReturnMetadata( UGCQueryHandle_t handle, bool bReturnMetadata) =>
            GetDelegate<SetReturnMetadataDelegate>()(InterfacePtr, handle, bReturnMetadata);

        #region VTableIndex(23)
        [VTableIndex(23), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnChildrenDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnChildren);
        #endregion
        public bool SetReturnChildren( UGCQueryHandle_t handle, bool bReturnChildren) =>
            GetDelegate<SetReturnChildrenDelegate>()(InterfacePtr, handle, bReturnChildren);

        #region VTableIndex(24)
        [VTableIndex(24), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnAdditionalPreviewsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnAdditionalPreviews);
        #endregion
        public bool SetReturnAdditionalPreviews( UGCQueryHandle_t handle, bool bReturnAdditionalPreviews) =>
            GetDelegate<SetReturnAdditionalPreviewsDelegate>()(InterfacePtr, handle, bReturnAdditionalPreviews);

        #region VTableIndex(25)
        [VTableIndex(25), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnTotalOnlyDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnTotalOnly);
        #endregion
        public bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool bReturnTotalOnly) =>
            GetDelegate<SetReturnTotalOnlyDelegate>()(InterfacePtr, handle, bReturnTotalOnly);

        #region VTableIndex(28)
        [VTableIndex(28), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetAllowCachedResponseDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds);
        #endregion
        public bool SetAllowCachedResponse(UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds) =>
            GetDelegate<SetAllowCachedResponseDelegate>()(InterfacePtr, handle, unMaxAgeSeconds);

        #region VTableIndex(29)
        [VTableIndex(29), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetCloudFileNameFilterDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pMatchCloudFileName);
        #endregion
        public bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName) =>
            GetDelegate<SetCloudFileNameFilterDelegate>()(InterfacePtr, handle, pMatchCloudFileName);

        #region VTableIndex(30)
        [VTableIndex(30), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetMatchAnyTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bMatchAnyTag);
        #endregion
        public bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag) =>
            GetDelegate<SetMatchAnyTagDelegate>()(InterfacePtr, handle, bMatchAnyTag);

        #region VTableIndex(31)
        [VTableIndex(31), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetSearchTextDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pSearchText);
        #endregion
        public bool SetSearchText(UGCQueryHandle_t handle, string pSearchText) =>
            GetDelegate<SetSearchTextDelegate>()(InterfacePtr, handle, pSearchText);

        #region VTableIndex(32)
        [VTableIndex(32), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetRankedByTrendDaysDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unDays);
        #endregion
        public bool SetRankedByTrendDays(UGCQueryHandle_t handle, UInt32 unDays) =>
            GetDelegate<SetRankedByTrendDaysDelegate>()(InterfacePtr, handle, unDays);

        [Obsolete("Not implemented in this version.", true)] public UInt64 RequestUGCDetails(UInt64 nPublishedFileID) { throw new NotImplementedException(); }

        #region VTableIndex(34)
        [VTableIndex(34), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RequestUGCDetailsDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds);
        #endregion
        [Obsolete("Use CreateQueryUGCDetailsRequest instead")]
        public UInt64 RequestUGCDetails(UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds) =>
            GetDelegate<RequestUGCDetailsDelegate>()(InterfacePtr, nPublishedFileID, unMaxAgeSeconds);

        #region VTableIndex(35)
        [VTableIndex(35), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateItemDelegate(IntPtr thisPtr, UInt32 nConsumerAppId, EWorkshopFileType eFileType);
        #endregion
        public UInt64 CreateItem(UInt32 nConsumerAppId, EWorkshopFileType eFileType) =>
            GetDelegate<CreateItemDelegate>()(InterfacePtr, nConsumerAppId, eFileType);

        #region VTableIndex(36)
        [VTableIndex(36), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StartItemUpdateDelegate(IntPtr thisPtr, UInt32 nConsumerAppId, UInt64 nPublishedFileID);
        #endregion
        public UInt64 StartItemUpdate(UInt32 nConsumerAppId, UInt64 nPublishedFileID) =>
            GetDelegate<StartItemUpdateDelegate>()(InterfacePtr, nConsumerAppId, nPublishedFileID);

        #region VTableIndex(37)
        [VTableIndex(37), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemTitleDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchTitle);
        #endregion
        public bool SetItemTitle(UInt64 handle, string pchTitle) =>
            GetDelegate<SetItemTitleDelegate>()(InterfacePtr, handle, pchTitle);

        #region VTableIndex(38)
        [VTableIndex(38), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemDescriptionDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchDescription);
        #endregion
        public bool SetItemDescription(UInt64 handle, string pchDescription) =>
            GetDelegate<SetItemDescriptionDelegate>()(InterfacePtr, handle, pchDescription);

        #region VTableIndex(40)
        [VTableIndex(40), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemMetadataDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchMetaData);
        #endregion
        public bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData) =>
            GetDelegate<SetItemMetadataDelegate>()(InterfacePtr, handle, pchMetaData);

        #region VTableIndex(41)
        [VTableIndex(41), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemVisibilityDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility);
        #endregion
        public bool SetItemVisibility(UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility) =>
            GetDelegate<SetItemVisibilityDelegate>()(InterfacePtr, handle, eVisibility);

        #region VTableIndex(42)
        [VTableIndex(42), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemTagsDelegate(IntPtr thisPtr, UGCUpdateHandle_t updateHandle, ref SteamParamStringArray_t pTags);
        #endregion
        public bool SetItemTags(UGCUpdateHandle_t updateHandle, ref SteamParamStringArray_t pTags) =>
            GetDelegate<SetItemTagsDelegate>()(InterfacePtr, updateHandle, ref pTags);

        #region VTableIndex(43)
        [VTableIndex(43), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemContentDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszContentFolder);
        #endregion
        public bool SetItemContent(UGCUpdateHandle_t handle, string pszContentFolder) =>
            GetDelegate<SetItemContentDelegate>()(InterfacePtr, handle, pszContentFolder);

        #region VTableIndex(44)
        [VTableIndex(44), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemPreviewDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszPreviewFile);
        #endregion
        public bool SetItemPreview(UGCUpdateHandle_t handle, string pszPreviewFile) =>
            GetDelegate<SetItemPreviewDelegate>()(InterfacePtr, handle, pszPreviewFile);

        #region VTableIndex(54)
        [VTableIndex(54), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SubmitItemUpdateDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchChangeNote);
        #endregion
        public UInt64 SubmitItemUpdate(UGCUpdateHandle_t handle, string pchChangeNote) =>
            GetDelegate<SubmitItemUpdateDelegate>()(InterfacePtr, handle, pchChangeNote);

        #region VTableIndex(55)
        [VTableIndex(55), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate EItemUpdateStatus GetItemUpdateProgressDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal);
        #endregion
        public EItemUpdateStatus GetItemUpdateProgress(UGCUpdateHandle_t handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal) =>
            GetDelegate<GetItemUpdateProgressDelegate>()(InterfacePtr, handle, ref punBytesProcessed, ref punBytesTotal);

        #region VTableIndex(58)
        [VTableIndex(58), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 AddItemToFavoritesDelegate(IntPtr thisPtr, UInt32 nCreatorAppID, UInt64 nPublishedFileID);
        #endregion
        public UInt64 AddItemToFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID) =>
            GetDelegate<AddItemToFavoritesDelegate>()(InterfacePtr, nCreatorAppID, nPublishedFileID);

        #region VTableIndex(59)
        [VTableIndex(59), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RemoveItemFromFavoritesDelegate(IntPtr thisPtr, UInt32 nCreatorAppID, UInt64 nPublishedFileID);
        #endregion
        public UInt64 RemoveItemFromFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID) =>
            GetDelegate<RemoveItemFromFavoritesDelegate>()(InterfacePtr, nCreatorAppID, nPublishedFileID);

        #region VTableIndex(60)
        [VTableIndex(60), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SubscribeItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        #endregion
        public UInt64 SubscribeItem(UInt64 nPublishedFileID) =>
            GetDelegate<SubscribeItemDelegate>()(InterfacePtr, nPublishedFileID);

        #region VTableIndex(61)
        [VTableIndex(61), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 UnsubscribeItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        #endregion
        public UInt64 UnsubscribeItem(UInt64 nPublishedFileID) =>
            GetDelegate<UnsubscribeItemDelegate>()(InterfacePtr, nPublishedFileID);

        #region VTableIndex(62)
        [VTableIndex(62), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetNumSubscribedItemsDelegate(IntPtr thisPtr);
        #endregion
        public UInt32 GetNumSubscribedItems() =>
            GetDelegate<GetNumSubscribedItemsDelegate>()(InterfacePtr);

        #region VTableIndex(63)
        [VTableIndex(63), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetSubscribedItemsDelegate(IntPtr thisPtr, [In, Out] UInt64[] pvecPublishedFileID, UInt32 cMaxEntries);
        #endregion
        public UInt32 GetSubscribedItems(UInt64[] pvecPublishedFileID, UInt32 cMaxEntries) =>
            GetDelegate<GetSubscribedItemsDelegate>()(InterfacePtr, pvecPublishedFileID, cMaxEntries);

        #region VTableIndex(64)
        [VTableIndex(64), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate EItemState GetItemStateDelegate(IntPtr thisPtr, UInt64 pvecPublishedFileID);
        #endregion
        public EItemState GetItemState(UInt64 nPublishedFileID) =>
            GetDelegate<GetItemStateDelegate>()(InterfacePtr, nPublishedFileID);

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref bool pbLegacyItem) { throw new NotImplementedException(); }

        #region VTableIndex(65)
        [VTableIndex(65), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetItemInstallInfoDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp);
        #endregion
        public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp) =>
            GetDelegate<GetItemInstallInfoDelegate>()(InterfacePtr, nPublishedFileID, ref punSizeOnDisk, pchFolder, cchFolderSize, ref punTimeStamp);

        [Obsolete("Not implemented in this version.", true)] public bool GetItemUpdateInfo(UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) { throw new NotImplementedException(); }

        #region VTableIndex(66)
        [VTableIndex(66), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetItemDownloadInfoDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);
        #endregion
        public bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) =>
            GetDelegate<GetItemDownloadInfoDelegate>()(InterfacePtr, nPublishedFileID, ref punBytesDownloaded, ref punBytesTotal);

        #region VTableIndex(67)
        [VTableIndex(67), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool DownloadItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, bool bHighPriority);
        #endregion
        public bool DownloadItem(UInt64 nPublishedFileID, bool bHighPriority) =>
            GetDelegate<DownloadItemDelegate>()(InterfacePtr, nPublishedFileID, bHighPriority);

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetQueryUGCNumTags(UGCQueryHandle_t handle, UInt32 index) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCTag(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCTagDisplayName(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize) { throw new NotImplementedException(); }

        #region VTableIndex(12)
        [VTableIndex(12), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetQueryUGCNumKeyValueTagsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index);
        #endregion
        public UInt32 GetQueryUGCNumKeyValueTags(UGCQueryHandle_t handle, UInt32 index) =>
            GetDelegate<GetQueryUGCNumKeyValueTagsDelegate>()(InterfacePtr, handle, index);

        #region VTableIndex(13)
        [VTableIndex(13), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCKeyValueTagByIndexDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, UInt32 keyValueTagIndex, StringBuilder pchKey, UInt32 cchKeySize, StringBuilder pchValue, UInt32 cchValueSize);
        #endregion
        public bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, UInt32 keyValueTagIndex, StringBuilder pchKey, UInt32 cchKeySize, StringBuilder pchValue, UInt32 cchValueSize) =>
            GetDelegate<GetQueryUGCKeyValueTagByIndexDelegate>()(InterfacePtr, handle, index, keyValueTagIndex, pchKey, cchKeySize, pchValue, cchValueSize);

        #region VTableIndex(14)
        [VTableIndex(14), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCKeyValueTagByKeyDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, string pchKey, StringBuilder pchValue, UInt32 cchValueSize);
        #endregion
        public bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, string pchKey, StringBuilder pchValue, UInt32 cchValueSize) =>
            GetDelegate<GetQueryUGCKeyValueTagByKeyDelegate>()(InterfacePtr, handle, index, pchKey, pchValue, cchValueSize);

        #region VTableIndex(17)
        [VTableIndex(17), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddRequiredTagGroupDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, ref SteamParamStringArray_t pTagGroups);
        #endregion
        public bool AddRequiredTagGroup(UGCQueryHandle_t handle, ref SteamParamStringArray_t pTagGroups) =>
            GetDelegate<AddRequiredTagGroupDelegate>()(InterfacePtr, handle, ref pTagGroups);

        #region VTableIndex(19)
        [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnOnlyIDsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnOnlyIDs);
        #endregion
        public bool SetReturnOnlyIDs(UGCQueryHandle_t handle, bool bReturnOnlyIDs) =>
            GetDelegate<SetReturnOnlyIDsDelegate>()(InterfacePtr, handle, bReturnOnlyIDs);

        #region VTableIndex(20)
        [VTableIndex(20), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnKeyValueTagsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnKeyValueTags);
        #endregion
        public bool SetReturnKeyValueTags(UGCQueryHandle_t handle, bool bReturnKeyValueTags) =>
            GetDelegate<SetReturnKeyValueTagsDelegate>()(InterfacePtr, handle, bReturnKeyValueTags);

        #region VTableIndex(26)
        [VTableIndex(26), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnPlaytimeStatsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unDays);
        #endregion
        public bool SetReturnPlaytimeStats(UGCQueryHandle_t handle, UInt32 unDays) =>
            GetDelegate<SetReturnPlaytimeStatsDelegate>()(InterfacePtr, handle, unDays);

        #region VTableIndex(27)
        [VTableIndex(27), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetLanguageDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pchLanguage);
        #endregion
        public bool SetLanguage(UGCQueryHandle_t handle, string pchLanguage) =>
            GetDelegate<SetLanguageDelegate>()(InterfacePtr, handle, pchLanguage);

        [Obsolete("Not implemented in this version.", true)] public bool SetTimeCreatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd) { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public bool SetTimeUpdatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd) { throw new NotImplementedException(); }

        #region VTableIndex(33)
        [VTableIndex(33), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddRequiredKeyValueTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pKey, string pValue);
        #endregion
        public bool AddRequiredKeyValueTag(UGCQueryHandle_t handle, string pKey, string pValue) =>
            GetDelegate<AddRequiredKeyValueTagDelegate>()(InterfacePtr, handle, pKey, pValue);

        #region VTableIndex(39)
        [VTableIndex(39), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemUpdateLanguageDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchLanguage);
        #endregion
        public bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, string pchLanguage) =>
            GetDelegate<SetItemUpdateLanguageDelegate>()(InterfacePtr, handle, pchLanguage);

        #region VTableIndex(45)
        [VTableIndex(45), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetAllowLegacyUploadDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, bool bAllowLegacyUpload);
        #endregion
        public bool SetAllowLegacyUpload(UGCUpdateHandle_t handle, bool bAllowLegacyUpload) =>
            GetDelegate<SetAllowLegacyUploadDelegate>()(InterfacePtr, handle, bAllowLegacyUpload);

        #region VTableIndex(46)
        [VTableIndex(46), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool RemoveAllItemKeyValueTagsDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle);
        #endregion
        public bool RemoveAllItemKeyValueTags(UGCUpdateHandle_t handle) =>
            GetDelegate<RemoveAllItemKeyValueTagsDelegate>()(InterfacePtr, handle);

        #region VTableIndex(47)
        [VTableIndex(47), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool RemoveItemKeyValueTagsDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchKey);
        #endregion
        public bool RemoveItemKeyValueTags(UGCUpdateHandle_t handle, string pchKey) =>
            GetDelegate<RemoveItemKeyValueTagsDelegate>()(InterfacePtr, handle, pchKey);

        #region VTableIndex(48)
        [VTableIndex(48), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddItemKeyValueTagDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchKey, string pchValue);
        #endregion
        public bool AddItemKeyValueTag(UGCUpdateHandle_t handle, string pchKey, string pchValue) =>
            GetDelegate<AddItemKeyValueTagDelegate>()(InterfacePtr, handle, pchKey, pchValue);

        #region VTableIndex(49)
        [VTableIndex(49), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddItemPreviewFileDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszPreviewFile, EItemPreviewType type);
        #endregion
        public bool AddItemPreviewFile(UGCUpdateHandle_t handle, string pszPreviewFile, EItemPreviewType type) =>
            GetDelegate<AddItemPreviewFileDelegate>()(InterfacePtr, handle, pszPreviewFile, type);

        #region VTableIndex(50)
        [VTableIndex(50), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddItemPreviewVideoDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszVideoID);
        #endregion
        public bool AddItemPreviewVideo(UGCUpdateHandle_t handle, string pszVideoID) =>
            GetDelegate<AddItemPreviewVideoDelegate>()(InterfacePtr, handle, pszVideoID);

        #region VTableIndex(51)
        [VTableIndex(51), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool UpdateItemPreviewFileDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, UInt32 index, string pszPreviewFile);
        #endregion
        public bool UpdateItemPreviewFile(UGCUpdateHandle_t handle, UInt32 index, string pszPreviewFile) =>
            GetDelegate<UpdateItemPreviewFileDelegate>()(InterfacePtr, handle, index, pszPreviewFile);

        #region VTableIndex(52)
        [VTableIndex(52), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool UpdateItemPreviewVideoDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, UInt32 index, string pszVideoID);
        #endregion
        public bool UpdateItemPreviewVideo(UGCUpdateHandle_t handle, UInt32 index, string pszVideoID) =>
            GetDelegate<UpdateItemPreviewVideoDelegate>()(InterfacePtr, handle, index, pszVideoID);

        #region VTableIndex(53)
        [VTableIndex(53), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool RemoveItemPreviewDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, UInt32 index);
        #endregion
        public bool RemoveItemPreview(UGCUpdateHandle_t handle, UInt32 index) =>
            GetDelegate<RemoveItemPreviewDelegate>()(InterfacePtr, handle, index);

        #region VTableIndex(56)
        [VTableIndex(56), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SetUserItemVoteDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID, bool bVoteUp);
        #endregion
        public UInt64 SetUserItemVote(PublishedFileId_t nPublishedFileID, bool bVoteUp) =>
            GetDelegate<SetUserItemVoteDelegate>()(InterfacePtr, nPublishedFileID, bVoteUp);

        #region VTableIndex(57)
        [VTableIndex(57), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 GetUserItemVoteDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID);
        #endregion
        public UInt64 GetUserItemVote(PublishedFileId_t nPublishedFileID) =>
            GetDelegate<GetUserItemVoteDelegate>()(InterfacePtr, nPublishedFileID);

        #region VTableIndex(68)
        [VTableIndex(68), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BInitWorkshopForGameServerDelegate(IntPtr thisPtr, UInt32 unWorkshopDepotID, string pszFolder);
        #endregion
        public bool BInitWorkshopForGameServer(UInt32 unWorkshopDepotID, string pszFolder) =>
            GetDelegate<BInitWorkshopForGameServerDelegate>()(InterfacePtr, unWorkshopDepotID, pszFolder);

        #region VTableIndex(69)
        [VTableIndex(69), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SuspendDownloadsDelegate(IntPtr thisPtr, bool bSuspend);
        #endregion
        public void SuspendDownloads(bool bSuspend) =>
            GetDelegate<SuspendDownloadsDelegate>()(InterfacePtr, bSuspend);

        #region VTableIndex(70)
        [VTableIndex(70), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StartPlaytimeTrackingDelegate(IntPtr thisPtr, [In] PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        #endregion
        public UInt64 StartPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs) =>
            GetDelegate<StartPlaytimeTrackingDelegate>()(InterfacePtr, pvecPublishedFileID, unNumPublishedFileIDs);

        #region VTableIndex(71)
        [VTableIndex(71), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StopPlaytimeTrackingDelegate(IntPtr thisPtr, [In] PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        #endregion
        public UInt64 StopPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs) =>
            GetDelegate<StopPlaytimeTrackingDelegate>()(InterfacePtr, pvecPublishedFileID, unNumPublishedFileIDs);

        #region VTableIndex(72)
        [VTableIndex(72), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StopPlaytimeTrackingForAllItemsDelegate(IntPtr thisPtr);
        #endregion
        public UInt64 StopPlaytimeTrackingForAllItems() =>
            GetDelegate<StopPlaytimeTrackingForAllItemsDelegate>()(InterfacePtr);

        #region VTableIndex(73)
        [VTableIndex(73), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 AddDependencyDelegate(IntPtr thisPtr, PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID);
        #endregion
        public UInt64 AddDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) =>
            GetDelegate<AddDependencyDelegate>()(InterfacePtr, nParentPublishedFileID, nChildPublishedFileID);

        #region VTableIndex(74)
        [VTableIndex(74), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RemoveDependencyDelegate(IntPtr thisPtr, PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID);
        #endregion
        public UInt64 RemoveDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) =>
            GetDelegate<RemoveDependencyDelegate>()(InterfacePtr, nParentPublishedFileID, nChildPublishedFileID);

        #region VTableIndex(75)
        [VTableIndex(75), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 AddAppDependencyDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID, UInt32 nAppID);
        #endregion
        public UInt64 AddAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID) =>
            GetDelegate<AddAppDependencyDelegate>()(InterfacePtr, nPublishedFileID, nAppID);

        #region VTableIndex(76)
        [VTableIndex(76), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RemoveAppDependencyDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID, UInt32 nAppID);
        #endregion
        public UInt64 RemoveAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID) =>
            GetDelegate<RemoveAppDependencyDelegate>()(InterfacePtr, nPublishedFileID, nAppID);

        #region VTableIndex(77)
        [VTableIndex(77), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 GetAppDependenciesDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID);
        #endregion
        public UInt64 GetAppDependencies(PublishedFileId_t nPublishedFileID) =>
            GetDelegate<GetAppDependenciesDelegate>()(InterfacePtr, nPublishedFileID);

        #region VTableIndex(78)
        [VTableIndex(78), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 DeleteItemDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID);
        #endregion
        public UInt64 DeleteItem(PublishedFileId_t nPublishedFileID) =>
            GetDelegate<DeleteItemDelegate>()(InterfacePtr, nPublishedFileID);

        [Obsolete("Not implemented in this version.", true)] public bool ShowWorkshopEULA() { throw new NotImplementedException(); }

        [Obsolete("Not implemented in this version.", true)] public UInt64 GetWorkshopEULAStatus() { throw new NotImplementedException(); }
    }
}
