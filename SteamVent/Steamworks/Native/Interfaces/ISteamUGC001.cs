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
    [InterfaceVersion("STEAMUGC_INTERFACE_VERSION001")]
    public class ISteamUGC001 : SteamInterfaceWrapper, ISteamUGC
    {
        public ISteamUGC001(IntPtr interfacePtr) : base(interfacePtr) { }

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
        private delegate bool SetCloudFileNameFilterDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pMatchCloudFileName);
        public bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName) => GetDelegate<SetCloudFileNameFilterDelegate>()(InterfacePtr, handle, pMatchCloudFileName);
        public FunctionImplemented SetCloudFileNameFilterSupport => FunctionImplemented.Implemented;

        [VTableIndex(10), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetMatchAnyTagDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, bool bMatchAnyTag);
        public bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag) => GetDelegate<SetMatchAnyTagDelegate>()(InterfacePtr, handle, bMatchAnyTag);
        public FunctionImplemented SetMatchAnyTagSupport => FunctionImplemented.Implemented;

        [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetSearchTextDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, string pSearchText);
        public bool SetSearchText(UGCQueryHandle_t handle, string pSearchText) => GetDelegate<SetSearchTextDelegate>()(InterfacePtr, handle, pSearchText);
        public FunctionImplemented SetSearchTextSupport => FunctionImplemented.Implemented;

        [VTableIndex(12), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool SetRankedByTrendDaysDelegate(IntPtr thisPtr, UGCQueryHandle_t handle, UInt32 unDays);
        public bool SetRankedByTrendDays(UGCQueryHandle_t handle, UInt32 unDays) => GetDelegate<SetRankedByTrendDaysDelegate>()(InterfacePtr, handle, unDays);
        public FunctionImplemented SetRankedByTrendDaysSupport => FunctionImplemented.Implemented;

        [VTableIndex(13), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt64 RequestUGCDetailsDelegate(IntPtr thisPtr, UInt64 nPublishedFileID);
        public UInt64 RequestUGCDetails(UInt64 nPublishedFileID) => GetDelegate<RequestUGCDetailsDelegate>()(InterfacePtr, nPublishedFileID);
        public FunctionImplemented RequestUGCDetailsSupport => FunctionImplemented.Implemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 RequestUGCDetailsAge(UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds) { throw new NotImplementedException(); }
        public FunctionImplemented RequestUGCDetailsAgeSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 CreateItem(UInt32 nConsumerAppId, EWorkshopFileType eFileType) { throw new NotImplementedException(); }
        public FunctionImplemented CreateItemSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 StartItemUpdate(UInt32 nConsumerAppId, UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented StartItemUpdateSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetItemTitle(UInt64 handle, string pchTitle) { throw new NotImplementedException(); }
        public FunctionImplemented SetItemTitleSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetItemDescription(UInt64 handle, string pchDescription) { throw new NotImplementedException(); }
        public FunctionImplemented SetItemDescriptionSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData) { throw new NotImplementedException(); }
        public FunctionImplemented SetItemMetadataSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetItemVisibility(UInt64 handle, ERemoteStoragePublishedFileVisibility eVisibility) { throw new NotImplementedException(); }
        public FunctionImplemented SetItemVisibilitySupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetItemTags(UInt64 updateHandle, ref SteamParamStringArray_t pTags) { throw new NotImplementedException(); }
        public FunctionImplemented SetItemTagsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetItemContent(UInt64 handle, string pszContentFolder) { throw new NotImplementedException(); }
        public FunctionImplemented SetItemContentSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool SetItemPreview(UInt64 handle, string pszPreviewFile) { throw new NotImplementedException(); }
        public FunctionImplemented SetItemPreviewSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 SubmitItemUpdate(UInt64 handle, string pchChangeNote) { throw new NotImplementedException(); }
        public FunctionImplemented SubmitItemUpdateSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public EItemUpdateStatus GetItemUpdateProgress(UInt64 handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemUpdateProgressSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 SubscribeItem(UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented SubscribeItemSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt64 UnsubscribeItem(UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented UnsubscribeItemSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetNumSubscribedItems() { throw new NotImplementedException(); }
        public FunctionImplemented GetNumSubscribedItemsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public UInt32 GetSubscribedItems(UInt64[] pvecPublishedFileID, UInt32 cMaxEntries) { throw new NotImplementedException(); }
        public FunctionImplemented GetSubscribedItemsSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public EItemState GetItemState(UInt64 nPublishedFileID) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemStateSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemInstallInfoSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfoLegacy(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref bool pbLegacyItem) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemInstallInfoLegacySupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemInstallInfoTimeStamp(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemInstallInfoTimeStampSupport => FunctionImplemented.NotImplemented;

        [Obsolete("Not implemented in this version.", true)] public bool GetItemUpdateInfo(UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal) { throw new NotImplementedException(); }
        public FunctionImplemented GetItemUpdateInfoSupport => FunctionImplemented.NotImplemented;

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

        [Obsolete("Not implemented in this version.", true)] public bool SetAllowCachedResponse(UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds) { throw new NotImplementedException(); }
        public FunctionImplemented SetAllowCachedResponseSupport => FunctionImplemented.NotImplemented;

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
