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
    /// Contains ISteamUGC010 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMUGC_INTERFACE_VERSION010")]
    public class ISteamUGC010 : SteamInterfaceWrapper, ISteamUGC
    {
        public ISteamUGC010(IntPtr interfacePtr) : base(interfacePtr) { }

        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateQueryUserUGCRequestDelegate(IntPtr thisPtr, UInt32 unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
        public UInt64 CreateQueryUserUGCRequest(UInt32 unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage) => GetDelegate<CreateQueryUserUGCRequestDelegate>()(InterfacePtr, unAccountID, eListType, eMatchingUGCType, eSortOrder, nCreatorAppID, nConsumerAppID, unPage);
        public FunctionImplemented CreateQueryUserUGCRequestSupport => FunctionImplemented.Implemented;

        [VTableIndex(1), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateQueryAllUGCRequestDelegate(IntPtr thisPtr, EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
        public UInt64 CreateQueryAllUGCRequestPage(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage) => GetDelegate<CreateQueryAllUGCRequestDelegate>()(InterfacePtr, eQueryType, eMatchingeMatchingUGCTypeFileType, nCreatorAppID, nConsumerAppID, unPage);
        public FunctionImplemented CreateQueryAllUGCRequestPageSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 CreateQueryAllUGCRequestCursor(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, string pchCursor) { throw new NotImplementedException(); }
        public FunctionImplemented CreateQueryAllUGCRequestCursorSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateQueryUGCDetailsRequestDelegate(IntPtr thisPtr, ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        public UInt64 CreateQueryUGCDetailsRequest(ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs) => GetDelegate<CreateQueryUGCDetailsRequestDelegate>()(InterfacePtr, ref pvecPublishedFileID, unNumPublishedFileIDs);
        public FunctionImplemented CreateQueryUGCDetailsRequestSupport => FunctionImplemented.Implemented;

        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SendQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        public UInt64 SendQueryUGCRequest(UGCQueryHandle_t handle) => GetDelegate<SendQueryUGCRequestDelegate>()(InterfacePtr, handle);
        public FunctionImplemented SendQueryUGCRequestSupport => FunctionImplemented.Implemented;

        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCResultDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails);
        public bool GetQueryUGCResult(UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails) => GetDelegate<GetQueryUGCResultDelegate>()(InterfacePtr, handle, index, ref pDetails);
        public FunctionImplemented GetQueryUGCResultSupport => FunctionImplemented.Implemented;

        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCPreviewURLDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize);
        public bool GetQueryUGCPreviewURL(UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize) => GetDelegate<GetQueryUGCPreviewURLDelegate>()(InterfacePtr, handle, index, ref pchURL, cchURLSize);
        public FunctionImplemented GetQueryUGCPreviewURLSupport => FunctionImplemented.Implemented;

        [VTableIndex(6), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCMetadataDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize);
        public bool GetQueryUGCMetadata(UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize) => GetDelegate<GetQueryUGCMetadataDelegate>()(InterfacePtr, handle, index, ref pchMetadata, cchMetadatasize);
        public FunctionImplemented GetQueryUGCMetadataSupport => FunctionImplemented.Implemented;

        [VTableIndex(7), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCChildrenDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries);
        public bool GetQueryUGCChildren(UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries) => GetDelegate<GetQueryUGCChildrenDelegate>()(InterfacePtr, handle, index, ref pvecPublishedFileID, cMaxEntries);
        public FunctionImplemented GetQueryUGCChildrenSupport => FunctionImplemented.Implemented;

        [VTableIndex(8), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCStatisticDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue);
        public bool GetQueryUGCStatistic(UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue) => GetDelegate<GetQueryUGCStatisticDelegate>()(InterfacePtr, handle, index, eStatType, ref pStatValue);
        public FunctionImplemented GetQueryUGCStatisticSupport => FunctionImplemented.Implemented;

        [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetQueryUGCNumAdditionalPreviewsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index);
        public UInt32 GetQueryUGCNumAdditionalPreviews(UGCQueryHandle_t handle, UInt32 index) => GetDelegate<GetQueryUGCNumAdditionalPreviewsDelegate>()(InterfacePtr, handle, index);
        public FunctionImplemented GetQueryUGCNumAdditionalPreviewsSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref bool pbIsImage) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCAdditionalPreviewSupport => FunctionImplemented.NotImplemented;

        // STEAMUGC_INTERFACE_VERSION008+: original file name + preview type replace the bool isImage flag.
        [VTableIndex(10), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCAdditionalPreviewFileDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref string pchOriginalFileName, UInt32 cchOriginalFileNameSize, ref EItemPreviewType pPreviewType);
        public bool GetQueryUGCAdditionalPreviewTyped(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref string pchOriginalFileName, UInt32 cchOriginalFileNameSize, ref EItemPreviewType pPreviewType) => GetDelegate<GetQueryUGCAdditionalPreviewFileDelegate>()(InterfacePtr, handle, index, previewIndex, ref pchURLOrVideoID, cchURLSize, ref pchOriginalFileName, cchOriginalFileNameSize, ref pPreviewType);
        public FunctionImplemented GetQueryUGCAdditionalPreviewTypedSupport => FunctionImplemented.Implemented;

        [VTableIndex(13), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool ReleaseQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        public bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle) => GetDelegate<ReleaseQueryUGCRequestDelegate>()(InterfacePtr, handle);
        public FunctionImplemented ReleaseQueryUGCRequestSupport => FunctionImplemented.Implemented;

        [VTableIndex(14), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddRequiredTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        public bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName) => GetDelegate<AddRequiredTagDelegate>()(InterfacePtr, handle, pTagName);
        public FunctionImplemented AddRequiredTagSupport => FunctionImplemented.Implemented;

        [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddExcludedTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        public bool AddExcludedTag(UGCQueryHandle_t handle, string pTagName) => GetDelegate<AddExcludedTagDelegate>()(InterfacePtr, handle, pTagName);
        public FunctionImplemented AddExcludedTagSupport => FunctionImplemented.Implemented;

        [VTableIndex(18), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnLongDescriptionDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnLongDescription);
        public bool SetReturnLongDescription(UGCQueryHandle_t handle, bool bReturnLongDescription) => GetDelegate<SetReturnLongDescriptionDelegate>()(InterfacePtr, handle, bReturnLongDescription);
        public FunctionImplemented SetReturnLongDescriptionSupport => FunctionImplemented.Implemented;

        [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnMetadataDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnMetadata);
        public bool SetReturnMetadata( UGCQueryHandle_t handle, bool bReturnMetadata) => GetDelegate<SetReturnMetadataDelegate>()(InterfacePtr, handle, bReturnMetadata);
        public FunctionImplemented SetReturnMetadataSupport => FunctionImplemented.Implemented;

        [VTableIndex(20), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnChildrenDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnChildren);
        public bool SetReturnChildren( UGCQueryHandle_t handle, bool bReturnChildren) => GetDelegate<SetReturnChildrenDelegate>()(InterfacePtr, handle, bReturnChildren);
        public FunctionImplemented SetReturnChildrenSupport => FunctionImplemented.Implemented;

        [VTableIndex(21), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnAdditionalPreviewsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnAdditionalPreviews);
        public bool SetReturnAdditionalPreviews( UGCQueryHandle_t handle, bool bReturnAdditionalPreviews) => GetDelegate<SetReturnAdditionalPreviewsDelegate>()(InterfacePtr, handle, bReturnAdditionalPreviews);
        public FunctionImplemented SetReturnAdditionalPreviewsSupport => FunctionImplemented.Implemented;

        [VTableIndex(22), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnTotalOnlyDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnTotalOnly);
        public bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool bReturnTotalOnly) => GetDelegate<SetReturnTotalOnlyDelegate>()(InterfacePtr, handle, bReturnTotalOnly);
        public FunctionImplemented SetReturnTotalOnlySupport => FunctionImplemented.Implemented;

        [VTableIndex(25), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetAllowCachedResponseDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds);
        public bool SetAllowCachedResponse(UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds) => GetDelegate<SetAllowCachedResponseDelegate>()(InterfacePtr, handle, unMaxAgeSeconds);
        public FunctionImplemented SetAllowCachedResponseSupport => FunctionImplemented.Implemented;

        [VTableIndex(26), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetCloudFileNameFilterDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pMatchCloudFileName);
        public bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName) => GetDelegate<SetCloudFileNameFilterDelegate>()(InterfacePtr, handle, pMatchCloudFileName);
        public FunctionImplemented SetCloudFileNameFilterSupport => FunctionImplemented.Implemented;

        [VTableIndex(27), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetMatchAnyTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bMatchAnyTag);
        public bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag) => GetDelegate<SetMatchAnyTagDelegate>()(InterfacePtr, handle, bMatchAnyTag);
        public FunctionImplemented SetMatchAnyTagSupport => FunctionImplemented.Implemented;

        [VTableIndex(28), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetSearchTextDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pSearchText);
        public bool SetSearchText(UGCQueryHandle_t handle, string pSearchText) => GetDelegate<SetSearchTextDelegate>()(InterfacePtr, handle, pSearchText);
        public FunctionImplemented SetSearchTextSupport => FunctionImplemented.Implemented;

        [VTableIndex(29), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetRankedByTrendDaysDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unDays);
        public bool SetRankedByTrendDays(UGCQueryHandle_t handle, UInt32 unDays) => GetDelegate<SetRankedByTrendDaysDelegate>()(InterfacePtr, handle, unDays);
        public FunctionImplemented SetRankedByTrendDaysSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 RequestUGCDetails(UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented RequestUGCDetailsSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(31), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RequestUGCDetailsDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds);
        [Obsolete("Use CreateQueryUGCDetailsRequest instead")]
        public UInt64 RequestUGCDetailsAge(UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds) => GetDelegate<RequestUGCDetailsDelegate>()(InterfacePtr, nPublishedFileID, unMaxAgeSeconds);
        public FunctionImplemented RequestUGCDetailsAgeSupport => FunctionImplemented.Implemented;

        [VTableIndex(32), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateItemDelegate(IntPtr thisPtr, UInt32 nConsumerAppId, EWorkshopFileType eFileType);
        public UInt64 CreateItem(UInt32 nConsumerAppId, EWorkshopFileType eFileType) => GetDelegate<CreateItemDelegate>()(InterfacePtr, nConsumerAppId, eFileType);
        public FunctionImplemented CreateItemSupport => FunctionImplemented.Implemented;

        [VTableIndex(33), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StartItemUpdateDelegate(IntPtr thisPtr, UInt32 nConsumerAppId, UInt64 nPublishedFileID);
        public UInt64 StartItemUpdate(UInt32 nConsumerAppId, UInt64 nPublishedFileID) => GetDelegate<StartItemUpdateDelegate>()(InterfacePtr, nConsumerAppId, nPublishedFileID);
        public FunctionImplemented StartItemUpdateSupport => FunctionImplemented.Implemented;

        [VTableIndex(34), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemTitleDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchTitle);
        public bool SetItemTitle(UInt64 handle, string pchTitle) => GetDelegate<SetItemTitleDelegate>()(InterfacePtr, handle, pchTitle);
        public FunctionImplemented SetItemTitleSupport => FunctionImplemented.Implemented;

        [VTableIndex(35), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemDescriptionDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchDescription);
        public bool SetItemDescription(UInt64 handle, string pchDescription) => GetDelegate<SetItemDescriptionDelegate>()(InterfacePtr, handle, pchDescription);
        public FunctionImplemented SetItemDescriptionSupport => FunctionImplemented.Implemented;

        [VTableIndex(37), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemMetadataDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchMetaData);
        public bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData) => GetDelegate<SetItemMetadataDelegate>()(InterfacePtr, handle, pchMetaData);
        public FunctionImplemented SetItemMetadataSupport => FunctionImplemented.Implemented;

        [VTableIndex(38), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemVisibilityDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility);
        public bool SetItemVisibility(UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility) => GetDelegate<SetItemVisibilityDelegate>()(InterfacePtr, handle, eVisibility);
        public FunctionImplemented SetItemVisibilitySupport => FunctionImplemented.Implemented;

        [VTableIndex(39), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemTagsDelegate(IntPtr thisPtr, UGCUpdateHandle_t updateHandle, ref SteamParamStringArray_t pTags);
        public bool SetItemTags(UGCUpdateHandle_t updateHandle, ref SteamParamStringArray_t pTags) => GetDelegate<SetItemTagsDelegate>()(InterfacePtr, updateHandle, ref pTags);
        public FunctionImplemented SetItemTagsSupport => FunctionImplemented.Implemented;

        [VTableIndex(40), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemContentDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszContentFolder);
        public bool SetItemContent(UGCUpdateHandle_t handle, string pszContentFolder) => GetDelegate<SetItemContentDelegate>()(InterfacePtr, handle, pszContentFolder);
        public FunctionImplemented SetItemContentSupport => FunctionImplemented.Implemented;

        [VTableIndex(41), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemPreviewDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszPreviewFile);
        public bool SetItemPreview(UGCUpdateHandle_t handle, string pszPreviewFile) => GetDelegate<SetItemPreviewDelegate>()(InterfacePtr, handle, pszPreviewFile);
        public FunctionImplemented SetItemPreviewSupport => FunctionImplemented.Implemented;

        [VTableIndex(49), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SubmitItemUpdateDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchChangeNote);
        public UInt64 SubmitItemUpdate(UGCUpdateHandle_t handle, string pchChangeNote) => GetDelegate<SubmitItemUpdateDelegate>()(InterfacePtr, handle, pchChangeNote);
        public FunctionImplemented SubmitItemUpdateSupport => FunctionImplemented.Implemented;

        [VTableIndex(50), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate EItemUpdateStatus GetItemUpdateProgressDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal);
        public EItemUpdateStatus GetItemUpdateProgress(UGCUpdateHandle_t handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal) => GetDelegate<GetItemUpdateProgressDelegate>()(InterfacePtr, handle, ref punBytesProcessed, ref punBytesTotal);
        public FunctionImplemented GetItemUpdateProgressSupport => FunctionImplemented.Implemented;

        [VTableIndex(53), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 AddItemToFavoritesDelegate(IntPtr thisPtr, UInt32 nCreatorAppID, UInt64 nPublishedFileID);
        public UInt64 AddItemToFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID) => GetDelegate<AddItemToFavoritesDelegate>()(InterfacePtr, nCreatorAppID, nPublishedFileID);
        public FunctionImplemented AddItemToFavoritesSupport => FunctionImplemented.Implemented;

        [VTableIndex(54), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RemoveItemFromFavoritesDelegate(IntPtr thisPtr, UInt32 nCreatorAppID, UInt64 nPublishedFileID);
        public UInt64 RemoveItemFromFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID) => GetDelegate<RemoveItemFromFavoritesDelegate>()(InterfacePtr, nCreatorAppID, nPublishedFileID);
        public FunctionImplemented RemoveItemFromFavoritesSupport => FunctionImplemented.Implemented;

        [VTableIndex(55), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SubscribeItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        public UInt64 SubscribeItem(UInt64 nPublishedFileID) => GetDelegate<SubscribeItemDelegate>()(InterfacePtr, nPublishedFileID);
        public FunctionImplemented SubscribeItemSupport => FunctionImplemented.Implemented;

        [VTableIndex(56), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 UnsubscribeItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        public UInt64 UnsubscribeItem(UInt64 nPublishedFileID) => GetDelegate<UnsubscribeItemDelegate>()(InterfacePtr, nPublishedFileID);
        public FunctionImplemented UnsubscribeItemSupport => FunctionImplemented.Implemented;

        [VTableIndex(57), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetNumSubscribedItemsDelegate(IntPtr thisPtr);
        public UInt32 GetNumSubscribedItems() => GetDelegate<GetNumSubscribedItemsDelegate>()(InterfacePtr);
        public FunctionImplemented GetNumSubscribedItemsSupport => FunctionImplemented.Implemented;

        [VTableIndex(58), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetSubscribedItemsDelegate(IntPtr thisPtr, [In, Out] UInt64[] pvecPublishedFileID, UInt32 cMaxEntries);
        public UInt32 GetSubscribedItems(UInt64[] pvecPublishedFileID, UInt32 cMaxEntries) => GetDelegate<GetSubscribedItemsDelegate>()(InterfacePtr, pvecPublishedFileID, cMaxEntries);
        public FunctionImplemented GetSubscribedItemsSupport => FunctionImplemented.Implemented;

        [VTableIndex(59), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate EItemState GetItemStateDelegate(IntPtr thisPtr, UInt64 pvecPublishedFileID);
        public EItemState GetItemState(UInt64 nPublishedFileID) => GetDelegate<GetItemStateDelegate>()(InterfacePtr, nPublishedFileID);
        public FunctionImplemented GetItemStateSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemInstallInfoSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfoLegacy(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref bool pbLegacyItem) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemInstallInfoLegacySupport => FunctionImplemented.NotImplemented;

        [VTableIndex(60), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetItemInstallInfoDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp);
        public bool GetItemInstallInfoTimeStamp(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp) => GetDelegate<GetItemInstallInfoDelegate>()(InterfacePtr, nPublishedFileID, ref punSizeOnDisk, pchFolder, cchFolderSize, ref punTimeStamp);
        public FunctionImplemented GetItemInstallInfoTimeStampSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemUpdateInfo(UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemUpdateInfoSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(61), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetItemDownloadInfoDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);
        public bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) => GetDelegate<GetItemDownloadInfoDelegate>()(InterfacePtr, nPublishedFileID, ref punBytesDownloaded, ref punBytesTotal);
        public FunctionImplemented GetItemDownloadInfoSupport => FunctionImplemented.Implemented;

        [VTableIndex(62), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool DownloadItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, bool bHighPriority);
        public bool DownloadItem(UInt64 nPublishedFileID, bool bHighPriority) => GetDelegate<DownloadItemDelegate>()(InterfacePtr, nPublishedFileID, bHighPriority);
        public FunctionImplemented DownloadItemSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetQueryUGCNumTags(UGCQueryHandle_t handle, UInt32 index) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCNumTagsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCTag(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCTagSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCTagDisplayName(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCTagDisplayNameSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetQueryUGCNumKeyValueTagsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index);
        public UInt32 GetQueryUGCNumKeyValueTags(UGCQueryHandle_t handle, UInt32 index) => GetDelegate<GetQueryUGCNumKeyValueTagsDelegate>()(InterfacePtr, handle, index);
        public FunctionImplemented GetQueryUGCNumKeyValueTagsSupport => FunctionImplemented.Implemented;

        [VTableIndex(12), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCKeyValueTagByIndexDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, UInt32 keyValueTagIndex, StringBuilder pchKey, UInt32 cchKeySize, StringBuilder pchValue, UInt32 cchValueSize);
        public bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, UInt32 keyValueTagIndex, StringBuilder pchKey, UInt32 cchKeySize, StringBuilder pchValue, UInt32 cchValueSize) => GetDelegate<GetQueryUGCKeyValueTagByIndexDelegate>()(InterfacePtr, handle, index, keyValueTagIndex, pchKey, cchKeySize, pchValue, cchValueSize);
        public FunctionImplemented GetQueryUGCKeyValueTagSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryFirstUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, string pchKey, StringBuilder pchValue, UInt32 cchValueSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryFirstUGCKeyValueTagSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool AddRequiredTagGroup(UGCQueryHandle_t handle, ref SteamParamStringArray_t pTagGroups) { throw new NotImplementedException(); }
        public FunctionImplemented AddRequiredTagGroupSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(16), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnOnlyIDsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnOnlyIDs);
        public bool SetReturnOnlyIDs(UGCQueryHandle_t handle, bool bReturnOnlyIDs) => GetDelegate<SetReturnOnlyIDsDelegate>()(InterfacePtr, handle, bReturnOnlyIDs);
        public FunctionImplemented SetReturnOnlyIDsSupport => FunctionImplemented.Implemented;

        [VTableIndex(17), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnKeyValueTagsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnKeyValueTags);
        public bool SetReturnKeyValueTags(UGCQueryHandle_t handle, bool bReturnKeyValueTags) => GetDelegate<SetReturnKeyValueTagsDelegate>()(InterfacePtr, handle, bReturnKeyValueTags);
        public FunctionImplemented SetReturnKeyValueTagsSupport => FunctionImplemented.Implemented;

        [VTableIndex(23), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnPlaytimeStatsDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unDays);
        public bool SetReturnPlaytimeStats(UGCQueryHandle_t handle, UInt32 unDays) => GetDelegate<SetReturnPlaytimeStatsDelegate>()(InterfacePtr, handle, unDays);
        public FunctionImplemented SetReturnPlaytimeStatsSupport => FunctionImplemented.Implemented;

        [VTableIndex(24), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetLanguageDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pchLanguage);
        public bool SetLanguage(UGCQueryHandle_t handle, string pchLanguage) => GetDelegate<SetLanguageDelegate>()(InterfacePtr, handle, pchLanguage);
        public FunctionImplemented SetLanguageSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetTimeCreatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd) { throw new NotImplementedException(); }
        public FunctionImplemented SetTimeCreatedDateRangeSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetTimeUpdatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd) { throw new NotImplementedException(); }
        public FunctionImplemented SetTimeUpdatedDateRangeSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(30), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddRequiredKeyValueTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pKey, string pValue);
        public bool AddRequiredKeyValueTag(UGCQueryHandle_t handle, string pKey, string pValue) => GetDelegate<AddRequiredKeyValueTagDelegate>()(InterfacePtr, handle, pKey, pValue);
        public FunctionImplemented AddRequiredKeyValueTagSupport => FunctionImplemented.Implemented;

        [VTableIndex(36), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemUpdateLanguageDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchLanguage);
        public bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, string pchLanguage) => GetDelegate<SetItemUpdateLanguageDelegate>()(InterfacePtr, handle, pchLanguage);
        public FunctionImplemented SetItemUpdateLanguageSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetAllowLegacyUpload(UGCUpdateHandle_t handle, bool bAllowLegacyUpload) { throw new NotImplementedException(); }
        public FunctionImplemented SetAllowLegacyUploadSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool RemoveAllItemKeyValueTags(UGCUpdateHandle_t handle) { throw new NotImplementedException(); }
        public FunctionImplemented RemoveAllItemKeyValueTagsSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(42), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool RemoveItemKeyValueTagsDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchKey);
        public bool RemoveItemKeyValueTags(UGCUpdateHandle_t handle, string pchKey) => GetDelegate<RemoveItemKeyValueTagsDelegate>()(InterfacePtr, handle, pchKey);
        public FunctionImplemented RemoveItemKeyValueTagsSupport => FunctionImplemented.Implemented;

        [VTableIndex(43), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddItemKeyValueTagDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pchKey, string pchValue);
        public bool AddItemKeyValueTag(UGCUpdateHandle_t handle, string pchKey, string pchValue) => GetDelegate<AddItemKeyValueTagDelegate>()(InterfacePtr, handle, pchKey, pchValue);
        public FunctionImplemented AddItemKeyValueTagSupport => FunctionImplemented.Implemented;

        [VTableIndex(44), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddItemPreviewFileDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszPreviewFile, EItemPreviewType type);
        public bool AddItemPreviewFile(UGCUpdateHandle_t handle, string pszPreviewFile, EItemPreviewType type) => GetDelegate<AddItemPreviewFileDelegate>()(InterfacePtr, handle, pszPreviewFile, type);
        public FunctionImplemented AddItemPreviewFileSupport => FunctionImplemented.Implemented;

        [VTableIndex(45), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddItemPreviewVideoDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, string pszVideoID);
        public bool AddItemPreviewVideo(UGCUpdateHandle_t handle, string pszVideoID) => GetDelegate<AddItemPreviewVideoDelegate>()(InterfacePtr, handle, pszVideoID);
        public FunctionImplemented AddItemPreviewVideoSupport => FunctionImplemented.Implemented;

        [VTableIndex(46), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool UpdateItemPreviewFileDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, UInt32 index, string pszPreviewFile);
        public bool UpdateItemPreviewFile(UGCUpdateHandle_t handle, UInt32 index, string pszPreviewFile) => GetDelegate<UpdateItemPreviewFileDelegate>()(InterfacePtr, handle, index, pszPreviewFile);
        public FunctionImplemented UpdateItemPreviewFileSupport => FunctionImplemented.Implemented;

        [VTableIndex(47), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool UpdateItemPreviewVideoDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, UInt32 index, string pszVideoID);
        public bool UpdateItemPreviewVideo(UGCUpdateHandle_t handle, UInt32 index, string pszVideoID) => GetDelegate<UpdateItemPreviewVideoDelegate>()(InterfacePtr, handle, index, pszVideoID);
        public FunctionImplemented UpdateItemPreviewVideoSupport => FunctionImplemented.Implemented;

        [VTableIndex(48), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool RemoveItemPreviewDelegate(IntPtr thisPtr, UGCUpdateHandle_t handle, UInt32 index);
        public bool RemoveItemPreview(UGCUpdateHandle_t handle, UInt32 index) => GetDelegate<RemoveItemPreviewDelegate>()(InterfacePtr, handle, index);
        public FunctionImplemented RemoveItemPreviewSupport => FunctionImplemented.Implemented;

        [VTableIndex(51), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SetUserItemVoteDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID, bool bVoteUp);
        public UInt64 SetUserItemVote(PublishedFileId_t nPublishedFileID, bool bVoteUp) => GetDelegate<SetUserItemVoteDelegate>()(InterfacePtr, nPublishedFileID, bVoteUp);
        public FunctionImplemented SetUserItemVoteSupport => FunctionImplemented.Implemented;

        [VTableIndex(52), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 GetUserItemVoteDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID);
        public UInt64 GetUserItemVote(PublishedFileId_t nPublishedFileID) => GetDelegate<GetUserItemVoteDelegate>()(InterfacePtr, nPublishedFileID);
        public FunctionImplemented GetUserItemVoteSupport => FunctionImplemented.Implemented;

        [VTableIndex(63), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BInitWorkshopForGameServerDelegate(IntPtr thisPtr, UInt32 unWorkshopDepotID, string pszFolder);
        public bool BInitWorkshopForGameServer(UInt32 unWorkshopDepotID, string pszFolder) => GetDelegate<BInitWorkshopForGameServerDelegate>()(InterfacePtr, unWorkshopDepotID, pszFolder);
        public FunctionImplemented BInitWorkshopForGameServerSupport => FunctionImplemented.Implemented;

        [VTableIndex(64), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SuspendDownloadsDelegate(IntPtr thisPtr, bool bSuspend);
        public void SuspendDownloads(bool bSuspend) => GetDelegate<SuspendDownloadsDelegate>()(InterfacePtr, bSuspend);
        public FunctionImplemented SuspendDownloadsSupport => FunctionImplemented.Implemented;

        [VTableIndex(65), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StartPlaytimeTrackingDelegate(IntPtr thisPtr, [In] PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        public UInt64 StartPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs) => GetDelegate<StartPlaytimeTrackingDelegate>()(InterfacePtr, pvecPublishedFileID, unNumPublishedFileIDs);
        public FunctionImplemented StartPlaytimeTrackingSupport => FunctionImplemented.Implemented;

        [VTableIndex(66), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StopPlaytimeTrackingDelegate(IntPtr thisPtr, [In] PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        public UInt64 StopPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs) => GetDelegate<StopPlaytimeTrackingDelegate>()(InterfacePtr, pvecPublishedFileID, unNumPublishedFileIDs);
        public FunctionImplemented StopPlaytimeTrackingSupport => FunctionImplemented.Implemented;

        [VTableIndex(67), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StopPlaytimeTrackingForAllItemsDelegate(IntPtr thisPtr);
        public UInt64 StopPlaytimeTrackingForAllItems() => GetDelegate<StopPlaytimeTrackingForAllItemsDelegate>()(InterfacePtr);
        public FunctionImplemented StopPlaytimeTrackingForAllItemsSupport => FunctionImplemented.Implemented;

        [VTableIndex(68), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 AddDependencyDelegate(IntPtr thisPtr, PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID);
        public UInt64 AddDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) => GetDelegate<AddDependencyDelegate>()(InterfacePtr, nParentPublishedFileID, nChildPublishedFileID);
        public FunctionImplemented AddDependencySupport => FunctionImplemented.Implemented;

        [VTableIndex(69), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RemoveDependencyDelegate(IntPtr thisPtr, PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID);
        public UInt64 RemoveDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) => GetDelegate<RemoveDependencyDelegate>()(InterfacePtr, nParentPublishedFileID, nChildPublishedFileID);
        public FunctionImplemented RemoveDependencySupport => FunctionImplemented.Implemented;

        [VTableIndex(70), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 AddAppDependencyDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID, UInt32 nAppID);
        public UInt64 AddAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID) => GetDelegate<AddAppDependencyDelegate>()(InterfacePtr, nPublishedFileID, nAppID);
        public FunctionImplemented AddAppDependencySupport => FunctionImplemented.Implemented;

        [VTableIndex(71), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RemoveAppDependencyDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID, UInt32 nAppID);
        public UInt64 RemoveAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID) => GetDelegate<RemoveAppDependencyDelegate>()(InterfacePtr, nPublishedFileID, nAppID);
        public FunctionImplemented RemoveAppDependencySupport => FunctionImplemented.Implemented;

        [VTableIndex(72), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 GetAppDependenciesDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID);
        public UInt64 GetAppDependencies(PublishedFileId_t nPublishedFileID) => GetDelegate<GetAppDependenciesDelegate>()(InterfacePtr, nPublishedFileID);
        public FunctionImplemented GetAppDependenciesSupport => FunctionImplemented.Implemented;

        [VTableIndex(73), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 DeleteItemDelegate(IntPtr thisPtr, PublishedFileId_t nPublishedFileID);
        public UInt64 DeleteItem(PublishedFileId_t nPublishedFileID) => GetDelegate<DeleteItemDelegate>()(InterfacePtr, nPublishedFileID);
        public FunctionImplemented DeleteItemSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool ShowWorkshopEULA() { throw new NotImplementedException(); }
        public FunctionImplemented ShowWorkshopEULASupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 GetWorkshopEULAStatus() { throw new NotImplementedException(); }
        public FunctionImplemented GetWorkshopEULAStatusSupport => FunctionImplemented.NotImplemented;
    }
}
