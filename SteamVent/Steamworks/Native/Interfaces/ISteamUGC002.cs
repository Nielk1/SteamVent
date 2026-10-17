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
    /// Contains ISteamApps008 delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("STEAMUGC_INTERFACE_VERSION002")]
    public class ISteamUGC002 : SteamInterfaceWrapper, ISteamUGC
    {
        public ISteamUGC002(IntPtr interfacePtr) : base(interfacePtr) { }

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

        [Obsolete("Not implemented in this version.", true)] public UInt64 CreateQueryUGCDetailsRequest(ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs) { throw new NotImplementedException(); }
        public FunctionImplemented CreateQueryUGCDetailsRequestSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SendQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        public UInt64 SendQueryUGCRequest(UGCQueryHandle_t handle) => GetDelegate<SendQueryUGCRequestDelegate>()(InterfacePtr, handle);
        public FunctionImplemented SendQueryUGCRequestSupport => FunctionImplemented.Implemented;

        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetQueryUGCResultDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails);
        public bool GetQueryUGCResult(UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails) => GetDelegate<GetQueryUGCResultDelegate>()(InterfacePtr, handle, index, ref pDetails);
        public FunctionImplemented GetQueryUGCResultSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCPreviewURL(UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCPreviewURLSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCMetadata(UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCMetadataSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCChildren(UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCChildrenSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCStatistic(UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCStatisticSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetQueryUGCNumAdditionalPreviews(UGCQueryHandle_t handle, UInt32 index) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCNumAdditionalPreviewsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref bool pbIsImage) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCAdditionalPreviewSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCAdditionalPreviewTyped(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref string pchOriginalFileName, UInt32 cchOriginalFileNameSize, ref EItemPreviewType pPreviewType) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCAdditionalPreviewTypedSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool ReleaseQueryUGCRequestDelegate(IntPtr thisPtr, UGCQueryHandle_t handle);
        public bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle) => GetDelegate<ReleaseQueryUGCRequestDelegate>()(InterfacePtr, handle);
        public FunctionImplemented ReleaseQueryUGCRequestSupport => FunctionImplemented.Implemented;

        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddRequiredTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        public bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName) => GetDelegate<AddRequiredTagDelegate>()(InterfacePtr, handle, pTagName);
        public FunctionImplemented AddRequiredTagSupport => FunctionImplemented.Implemented;

        [VTableIndex(6), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool AddExcludedTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pTagName);
        public bool AddExcludedTag(UGCQueryHandle_t handle, string pTagName) => GetDelegate<AddExcludedTagDelegate>()(InterfacePtr, handle, pTagName);
        public FunctionImplemented AddExcludedTagSupport => FunctionImplemented.Implemented;

        [VTableIndex(7), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnLongDescriptionDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnLongDescription);
        public bool SetReturnLongDescription(UGCQueryHandle_t handle, bool bReturnLongDescription) => GetDelegate<SetReturnLongDescriptionDelegate>()(InterfacePtr, handle, bReturnLongDescription);
        public FunctionImplemented SetReturnLongDescriptionSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetReturnMetadata(UGCQueryHandle_t handle, bool bReturnMetadata) { throw new NotImplementedException(); }
        public FunctionImplemented SetReturnMetadataSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetReturnChildren(UGCQueryHandle_t handle, bool bReturnChildren) { throw new NotImplementedException(); }
        public FunctionImplemented SetReturnChildrenSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetReturnAdditionalPreviews(UGCQueryHandle_t handle, bool bReturnAdditionalPreviews) { throw new NotImplementedException(); }
        public FunctionImplemented SetReturnAdditionalPreviewsSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(8), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetReturnTotalOnlyDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bReturnTotalOnly);
        public bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool bReturnTotalOnly) => GetDelegate<SetReturnTotalOnlyDelegate>()(InterfacePtr, handle, bReturnTotalOnly);
        public FunctionImplemented SetReturnTotalOnlySupport => FunctionImplemented.Implemented;

        [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetAllowCachedResponseDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds);
        public bool SetAllowCachedResponse(UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds) => GetDelegate<SetAllowCachedResponseDelegate>()(InterfacePtr, handle, unMaxAgeSeconds);
        public FunctionImplemented SetAllowCachedResponseSupport => FunctionImplemented.Implemented;

        [VTableIndex(10), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetCloudFileNameFilterDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pMatchCloudFileName);
        public bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName) => GetDelegate<SetCloudFileNameFilterDelegate>()(InterfacePtr, handle, pMatchCloudFileName);
        public FunctionImplemented SetCloudFileNameFilterSupport => FunctionImplemented.Implemented;

        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetMatchAnyTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bMatchAnyTag);
        public bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag) => GetDelegate<SetMatchAnyTagDelegate>()(InterfacePtr, handle, bMatchAnyTag);
        public FunctionImplemented SetMatchAnyTagSupport => FunctionImplemented.Implemented;

        [VTableIndex(12), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetSearchTextDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pSearchText);
        public bool SetSearchText(UGCQueryHandle_t handle, string pSearchText) => GetDelegate<SetSearchTextDelegate>()(InterfacePtr, handle, pSearchText);
        public FunctionImplemented SetSearchTextSupport => FunctionImplemented.Implemented;

        [VTableIndex(13), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetRankedByTrendDaysDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unDays);
        public bool SetRankedByTrendDays(UGCQueryHandle_t handle, UInt32 unDays) => GetDelegate<SetRankedByTrendDaysDelegate>()(InterfacePtr, handle, unDays);
        public FunctionImplemented SetRankedByTrendDaysSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 RequestUGCDetails(UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented RequestUGCDetailsSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(14), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RequestUGCDetailsDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds);
        public UInt64 RequestUGCDetailsAge(UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds) => GetDelegate<RequestUGCDetailsDelegate>()(InterfacePtr, nPublishedFileID, unMaxAgeSeconds);
        public FunctionImplemented RequestUGCDetailsAgeSupport => FunctionImplemented.Implemented;


        [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 CreateItemDelegate(IntPtr thisPtr, UInt32 nConsumerAppId, EWorkshopFileType eFileType);
        public UInt64 CreateItem(UInt32 nConsumerAppId, EWorkshopFileType eFileType) => GetDelegate<CreateItemDelegate>()(InterfacePtr, nConsumerAppId, eFileType);
        public FunctionImplemented CreateItemSupport => FunctionImplemented.Implemented;

        [VTableIndex(16), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 StartItemUpdateDelegate(IntPtr thisPtr, UInt32 nConsumerAppId, UInt64 nPublishedFileID);
        public UInt64 StartItemUpdate(UInt32 nConsumerAppId, UInt64 nPublishedFileID) => GetDelegate<StartItemUpdateDelegate>()(InterfacePtr, nConsumerAppId, nPublishedFileID);
        public FunctionImplemented StartItemUpdateSupport => FunctionImplemented.Implemented;

        [VTableIndex(17), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemTitleDelegate(IntPtr thisPtr, UInt64 handle, string pchTitle);
        public bool SetItemTitle(UInt64 handle, string pchTitle) => GetDelegate<SetItemTitleDelegate>()(InterfacePtr, handle, pchTitle);
        public FunctionImplemented SetItemTitleSupport => FunctionImplemented.Implemented;

        [VTableIndex(18), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemDescriptionDelegate(IntPtr thisPtr, UInt64 handle, string pchDescription);
        public bool SetItemDescription(UInt64 handle, string pchDescription) => GetDelegate<SetItemDescriptionDelegate>()(InterfacePtr, handle, pchDescription);
        public FunctionImplemented SetItemDescriptionSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData) { throw new NotImplementedException(); }
        public FunctionImplemented SetItemMetadataSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemVisibilityDelegate(IntPtr thisPtr, UInt64 handle, ERemoteStoragePublishedFileVisibility eVisibility);
        public bool SetItemVisibility(UInt64 handle, ERemoteStoragePublishedFileVisibility eVisibility) => GetDelegate<SetItemVisibilityDelegate>()(InterfacePtr, handle, eVisibility);
        public FunctionImplemented SetItemVisibilitySupport => FunctionImplemented.Implemented;

        [VTableIndex(20), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemTagsDelegate(IntPtr thisPtr, UInt64 updateHandle, ref SteamParamStringArray_t pTags);
        public bool SetItemTags(UInt64 updateHandle, ref SteamParamStringArray_t pTags) => GetDelegate<SetItemTagsDelegate>()(InterfacePtr, updateHandle, ref pTags);
        public FunctionImplemented SetItemTagsSupport => FunctionImplemented.Implemented;

        [VTableIndex(21), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemContentDelegate(IntPtr thisPtr, UInt64 handle, string pszContentFolder);
        public bool SetItemContent(UInt64 handle, string pszContentFolder) => GetDelegate<SetItemContentDelegate>()(InterfacePtr, handle, pszContentFolder);
        public FunctionImplemented SetItemContentSupport => FunctionImplemented.Implemented;

        [VTableIndex(22), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetItemPreviewDelegate(IntPtr thisPtr, UInt64 handle, string pszPreviewFile);
        public bool SetItemPreview(UInt64 handle, string pszPreviewFile) => GetDelegate<SetItemPreviewDelegate>()(InterfacePtr, handle, pszPreviewFile);
        public FunctionImplemented SetItemPreviewSupport => FunctionImplemented.Implemented;

        [VTableIndex(23), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SubmitItemUpdateDelegate(IntPtr thisPtr, UInt64 handle, string pchChangeNote);
        public UInt64 SubmitItemUpdate(UInt64 handle, string pchChangeNote) => GetDelegate<SubmitItemUpdateDelegate>()(InterfacePtr, handle, pchChangeNote);
        public FunctionImplemented SubmitItemUpdateSupport => FunctionImplemented.Implemented;

        [VTableIndex(24), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate EItemUpdateStatus GetItemUpdateProgressDelegate(IntPtr thisPtr, UInt64 handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal);
        public EItemUpdateStatus GetItemUpdateProgress(UInt64 handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal) => GetDelegate<GetItemUpdateProgressDelegate>()(InterfacePtr, handle, ref punBytesProcessed, ref punBytesTotal);
        public FunctionImplemented GetItemUpdateProgressSupport => FunctionImplemented.Implemented;

        [VTableIndex(25), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 SubscribeItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        public UInt64 SubscribeItem(UInt64 nPublishedFileID) => GetDelegate<SubscribeItemDelegate>()(InterfacePtr, nPublishedFileID);
        public FunctionImplemented SubscribeItemSupport => FunctionImplemented.Implemented;

        [VTableIndex(26), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 UnsubscribeItemDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        public UInt64 UnsubscribeItem(UInt64 nPublishedFileID) => GetDelegate<UnsubscribeItemDelegate>()(InterfacePtr, nPublishedFileID);
        public FunctionImplemented UnsubscribeItemSupport => FunctionImplemented.Implemented;

        [VTableIndex(27), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetNumSubscribedItemsDelegate(IntPtr thisPtr);
        public UInt32 GetNumSubscribedItems() => GetDelegate<GetNumSubscribedItemsDelegate>()(InterfacePtr);
        public FunctionImplemented GetNumSubscribedItemsSupport => FunctionImplemented.Implemented;

        [VTableIndex(28), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetSubscribedItemsDelegate(IntPtr thisPtr, [In, Out] UInt64[] pvecPublishedFileID, UInt32 cMaxEntries);
        public UInt32 GetSubscribedItems(UInt64[] pvecPublishedFileID, UInt32 cMaxEntries) => GetDelegate<GetSubscribedItemsDelegate>()(InterfacePtr, pvecPublishedFileID, cMaxEntries);
        public FunctionImplemented GetSubscribedItemsSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public EItemState GetItemState(UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemStateSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(29), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetItemInstallInfoDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize);
        public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize) => GetDelegate<GetItemInstallInfoDelegate>()(InterfacePtr, nPublishedFileID, ref punSizeOnDisk, pchFolder, cchFolderSize);
        public FunctionImplemented GetItemInstallInfoSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfoLegacy(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref bool pbLegacyItem) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemInstallInfoLegacySupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfoTimeStamp(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemInstallInfoTimeStampSupport => FunctionImplemented.NotImplemented;

        [VTableIndex(30), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool GetItemUpdateInfoDelegate(IntPtr thisPtr, UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);
        public bool GetItemUpdateInfo(UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) => GetDelegate<GetItemUpdateInfoDelegate>()(InterfacePtr, nPublishedFileID, ref pbNeedsUpdate, ref pbIsDownloading, ref punBytesDownloaded, ref punBytesTotal);
        public FunctionImplemented GetItemUpdateInfoSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemDownloadInfoSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool DownloadItem(UInt64 nPublishedFileID, bool bHighPriority) { throw new NotImplementedException(); }
        public FunctionImplemented DownloadItemSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 AddItemToFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented AddItemToFavoritesSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 RemoveItemFromFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented RemoveItemFromFavoritesSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetQueryUGCNumTags(UGCQueryHandle_t handle, UInt32 index) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCNumTagsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCTag(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCTagSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCTagDisplayName(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCTagDisplayNameSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetQueryUGCNumKeyValueTags(UGCQueryHandle_t handle, UInt32 index) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCNumKeyValueTagsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, UInt32 keyValueTagIndex, StringBuilder pchKey, UInt32 cchKeySize, StringBuilder pchValue, UInt32 cchValueSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryUGCKeyValueTagSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetQueryFirstUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, string pchKey, StringBuilder pchValue, UInt32 cchValueSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetQueryFirstUGCKeyValueTagSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool AddRequiredTagGroup(UGCQueryHandle_t handle, ref SteamParamStringArray_t pTagGroups) { throw new NotImplementedException(); }
        public FunctionImplemented AddRequiredTagGroupSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetReturnOnlyIDs(UGCQueryHandle_t handle, bool bReturnOnlyIDs) { throw new NotImplementedException(); }
        public FunctionImplemented SetReturnOnlyIDsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetReturnKeyValueTags(UGCQueryHandle_t handle, bool bReturnKeyValueTags) { throw new NotImplementedException(); }
        public FunctionImplemented SetReturnKeyValueTagsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetReturnPlaytimeStats(UGCQueryHandle_t handle, UInt32 unDays) { throw new NotImplementedException(); }
        public FunctionImplemented SetReturnPlaytimeStatsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetLanguage(UGCQueryHandle_t handle, string pchLanguage) { throw new NotImplementedException(); }
        public FunctionImplemented SetLanguageSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetTimeCreatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd) { throw new NotImplementedException(); }
        public FunctionImplemented SetTimeCreatedDateRangeSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetTimeUpdatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd) { throw new NotImplementedException(); }
        public FunctionImplemented SetTimeUpdatedDateRangeSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool AddRequiredKeyValueTag(UGCQueryHandle_t handle, string pKey, string pValue) { throw new NotImplementedException(); }
        public FunctionImplemented AddRequiredKeyValueTagSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, string pchLanguage) { throw new NotImplementedException(); }
        public FunctionImplemented SetItemUpdateLanguageSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetAllowLegacyUpload(UGCUpdateHandle_t handle, bool bAllowLegacyUpload) { throw new NotImplementedException(); }
        public FunctionImplemented SetAllowLegacyUploadSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool RemoveAllItemKeyValueTags(UGCUpdateHandle_t handle) { throw new NotImplementedException(); }
        public FunctionImplemented RemoveAllItemKeyValueTagsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool RemoveItemKeyValueTags(UGCUpdateHandle_t handle, string pchKey) { throw new NotImplementedException(); }
        public FunctionImplemented RemoveItemKeyValueTagsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool AddItemKeyValueTag(UGCUpdateHandle_t handle, string pchKey, string pchValue) { throw new NotImplementedException(); }
        public FunctionImplemented AddItemKeyValueTagSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool AddItemPreviewFile(UGCUpdateHandle_t handle, string pszPreviewFile, EItemPreviewType type) { throw new NotImplementedException(); }
        public FunctionImplemented AddItemPreviewFileSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool AddItemPreviewVideo(UGCUpdateHandle_t handle, string pszVideoID) { throw new NotImplementedException(); }
        public FunctionImplemented AddItemPreviewVideoSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool UpdateItemPreviewFile(UGCUpdateHandle_t handle, UInt32 index, string pszPreviewFile) { throw new NotImplementedException(); }
        public FunctionImplemented UpdateItemPreviewFileSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool UpdateItemPreviewVideo(UGCUpdateHandle_t handle, UInt32 index, string pszVideoID) { throw new NotImplementedException(); }
        public FunctionImplemented UpdateItemPreviewVideoSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool RemoveItemPreview(UGCUpdateHandle_t handle, UInt32 index) { throw new NotImplementedException(); }
        public FunctionImplemented RemoveItemPreviewSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 SetUserItemVote(PublishedFileId_t nPublishedFileID, bool bVoteUp) { throw new NotImplementedException(); }
        public FunctionImplemented SetUserItemVoteSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 GetUserItemVote(PublishedFileId_t nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented GetUserItemVoteSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool BInitWorkshopForGameServer(UInt32 unWorkshopDepotID, string pszFolder) { throw new NotImplementedException(); }
        public FunctionImplemented BInitWorkshopForGameServerSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public void SuspendDownloads(bool bSuspend) { throw new NotImplementedException(); }
        public FunctionImplemented SuspendDownloadsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 StartPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs) { throw new NotImplementedException(); }
        public FunctionImplemented StartPlaytimeTrackingSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 StopPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs) { throw new NotImplementedException(); }
        public FunctionImplemented StopPlaytimeTrackingSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 StopPlaytimeTrackingForAllItems() { throw new NotImplementedException(); }
        public FunctionImplemented StopPlaytimeTrackingForAllItemsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 AddDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented AddDependencySupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 RemoveDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented RemoveDependencySupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 AddAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID) { throw new NotImplementedException(); }
        public FunctionImplemented AddAppDependencySupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 RemoveAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID) { throw new NotImplementedException(); }
        public FunctionImplemented RemoveAppDependencySupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 GetAppDependencies(PublishedFileId_t nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented GetAppDependenciesSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 DeleteItem(PublishedFileId_t nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented DeleteItemSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool ShowWorkshopEULA() { throw new NotImplementedException(); }
        public FunctionImplemented ShowWorkshopEULASupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 GetWorkshopEULAStatus() { throw new NotImplementedException(); }
        public FunctionImplemented GetWorkshopEULAStatusSupport => FunctionImplemented.NotImplemented;
    }
}
