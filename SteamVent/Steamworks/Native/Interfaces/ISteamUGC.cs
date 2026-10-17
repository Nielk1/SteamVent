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
    public interface ISteamUGC
    {
        static readonly public Type[] Versions =
        {
            typeof(ISteamUGC001),
            typeof(ISteamUGC002),
            typeof(ISteamUGC003),
            typeof(ISteamUGC004),
            typeof(ISteamUGC005),
            typeof(ISteamUGC006),
            typeof(ISteamUGC007),
            typeof(ISteamUGC008),
            typeof(ISteamUGC009),
            typeof(ISteamUGC010),
            //typeof(ISteamUGC011),
            typeof(ISteamUGC012),
            typeof(ISteamUGC013),
            typeof(ISteamUGC014),
            typeof(ISteamUGC015),
            typeof(ISteamUGC016),
        };

        UInt64 CreateQueryUserUGCRequest(UInt32 unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
        FunctionImplemented CreateQueryUserUGCRequestSupport { get; }

        UInt64 CreateQueryAllUGCRequestPage(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
        FunctionImplemented CreateQueryAllUGCRequestPageSupport { get; }

        // Deep-paging overload (STEAMUGC_INTERFACE_VERSION012+): pchCursor is null or "*" for the first page.
        UInt64 CreateQueryAllUGCRequestCursor(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, string pchCursor);
        FunctionImplemented CreateQueryAllUGCRequestCursorSupport { get; }

        UInt64 CreateQueryUGCDetailsRequest(ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        FunctionImplemented CreateQueryUGCDetailsRequestSupport { get; }

        UInt64 SendQueryUGCRequest(UGCQueryHandle_t handle);
        FunctionImplemented SendQueryUGCRequestSupport { get; }

        bool GetQueryUGCResult(UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails);
        FunctionImplemented GetQueryUGCResultSupport { get; }

        bool GetQueryUGCPreviewURL(UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize);
        FunctionImplemented GetQueryUGCPreviewURLSupport { get; }

        bool GetQueryUGCMetadata(UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize);
        FunctionImplemented GetQueryUGCMetadataSupport { get; }

        bool GetQueryUGCChildren(UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries);
        FunctionImplemented GetQueryUGCChildrenSupport { get; }

        bool GetQueryUGCStatistic(UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue);
        FunctionImplemented GetQueryUGCStatisticSupport { get; }

        UInt32 GetQueryUGCNumAdditionalPreviews(UGCQueryHandle_t handle, UInt32 index);
        FunctionImplemented GetQueryUGCNumAdditionalPreviewsSupport { get; }
        
        // STEAMUGC_INTERFACE_VERSION005-007: the last parameter is a `bool` "is image" flag.
        bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref bool pbIsImage);
        FunctionImplemented GetQueryUGCAdditionalPreviewSupport { get; }

        // STEAMUGC_INTERFACE_VERSION008+: that `bool` is replaced by the original file name and the
        // preview type. Same vtable slot, different signature - hence a second overload of the
        // same name (the parameter list disambiguates the two contracts).
        bool GetQueryUGCAdditionalPreviewTyped(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref string pchOriginalFileName, UInt32 cchOriginalFileNameSize, ref EItemPreviewType pPreviewType);
        FunctionImplemented GetQueryUGCAdditionalPreviewTypedSupport { get; }

        bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle);
        FunctionImplemented ReleaseQueryUGCRequestSupport { get; }

        bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName);
        FunctionImplemented AddRequiredTagSupport { get; }

        bool AddExcludedTag(UGCQueryHandle_t handle, string pTagName);
        FunctionImplemented AddExcludedTagSupport { get; }

        bool SetReturnLongDescription(UGCQueryHandle_t handle, bool bReturnLongDescription);
        FunctionImplemented SetReturnLongDescriptionSupport { get; }

        bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool bReturnTotalOnly);
        FunctionImplemented SetReturnTotalOnlySupport { get; }

        bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName);
        FunctionImplemented SetCloudFileNameFilterSupport { get; }

        bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag);
        FunctionImplemented SetMatchAnyTagSupport { get; }

        bool SetSearchText(UGCQueryHandle_t handle, string pSearchText);
        FunctionImplemented SetSearchTextSupport { get; }

        bool SetRankedByTrendDays(UGCQueryHandle_t handle, UInt32 unDays);
        FunctionImplemented SetRankedByTrendDaysSupport { get; }

        // TODO handle this interface change better 
        UInt64 RequestUGCDetails(UInt64 nPublishedFileID);
        FunctionImplemented RequestUGCDetailsSupport { get; }

        UInt64 RequestUGCDetailsAge(UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds);
        FunctionImplemented RequestUGCDetailsAgeSupport { get; }

        UInt64 CreateItem(UInt32 nConsumerAppId, EWorkshopFileType eFileType);
        FunctionImplemented CreateItemSupport { get; }

        UInt64 StartItemUpdate(UInt32 nConsumerAppId, UInt64 nPublishedFileID);
        FunctionImplemented StartItemUpdateSupport { get; }

        bool SetItemTitle(UInt64 handle, string pchTitle);
        FunctionImplemented SetItemTitleSupport { get; }

        bool SetItemDescription(UInt64 handle, string pchDescription);
        FunctionImplemented SetItemDescriptionSupport { get; }

        bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData);
        FunctionImplemented SetItemMetadataSupport { get; }

        bool SetItemVisibility(UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility);
        FunctionImplemented SetItemVisibilitySupport { get; }

        bool SetItemTags(UGCUpdateHandle_t updateHandle, ref SteamParamStringArray_t pTags);
        FunctionImplemented SetItemTagsSupport { get; }

        bool SetItemContent(UGCUpdateHandle_t handle, string pszContentFolder);
        FunctionImplemented SetItemContentSupport { get; }

        bool SetItemPreview(UGCUpdateHandle_t handle, string pszPreviewFile);
        FunctionImplemented SetItemPreviewSupport { get; }

        UInt64 SubmitItemUpdate(UGCUpdateHandle_t handle, string pchChangeNote);
        FunctionImplemented SubmitItemUpdateSupport { get; }

        EItemUpdateStatus GetItemUpdateProgress(UGCUpdateHandle_t handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal);
        FunctionImplemented GetItemUpdateProgressSupport { get; }

        UInt64 AddItemToFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID);
        FunctionImplemented AddItemToFavoritesSupport { get; }

        UInt64 RemoveItemFromFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID);
        FunctionImplemented RemoveItemFromFavoritesSupport { get; }

        UInt64 SubscribeItem(UInt64 nPublishedFileID);
        FunctionImplemented SubscribeItemSupport { get; }

        UInt64 UnsubscribeItem(UInt64 nPublishedFileID);
        FunctionImplemented UnsubscribeItemSupport { get; }

        UInt32 GetNumSubscribedItems();
        FunctionImplemented GetNumSubscribedItemsSupport { get; }

        UInt32 GetSubscribedItems(UInt64[] pvecPublishedFileID, UInt32 cMaxEntries);
        FunctionImplemented GetSubscribedItemsSupport { get; }

        EItemState GetItemState(UInt64 nPublishedFileID);
        FunctionImplemented GetItemStateSupport { get; }

        bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize);
        FunctionImplemented GetItemInstallInfoSupport { get; }

        bool GetItemInstallInfoLegacy(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref bool pbLegacyItem);
        FunctionImplemented GetItemInstallInfoLegacySupport { get; }

        bool GetItemInstallInfoTimeStamp(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp);
        FunctionImplemented GetItemInstallInfoTimeStampSupport { get; }

        bool GetItemUpdateInfo(UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);
        FunctionImplemented GetItemUpdateInfoSupport { get; }

        bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);
        FunctionImplemented GetItemDownloadInfoSupport { get; }

        bool DownloadItem(UInt64 nPublishedFileID, bool bHighPriority);
        FunctionImplemented DownloadItemSupport { get; }

        UInt32 GetQueryUGCNumTags(UGCQueryHandle_t handle, UInt32 index);
        FunctionImplemented GetQueryUGCNumTagsSupport { get; }

        bool GetQueryUGCTag(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize);
        FunctionImplemented GetQueryUGCTagSupport { get; }

        bool GetQueryUGCTagDisplayName(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize);
        FunctionImplemented GetQueryUGCTagDisplayNameSupport { get; }

        UInt32 GetQueryUGCNumKeyValueTags(UGCQueryHandle_t handle, UInt32 index);
        FunctionImplemented GetQueryUGCNumKeyValueTagsSupport { get; }

        bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, UInt32 keyValueTagIndex, StringBuilder pchKey, UInt32 cchKeySize, StringBuilder pchValue, UInt32 cchValueSize);
        FunctionImplemented GetQueryUGCKeyValueTagSupport { get; }

        bool GetQueryFirstUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, string pchKey, StringBuilder pchValue, UInt32 cchValueSize);
        FunctionImplemented GetQueryFirstUGCKeyValueTagSupport { get; }

        bool AddRequiredTagGroup(UGCQueryHandle_t handle, ref SteamParamStringArray_t pTagGroups);
        FunctionImplemented AddRequiredTagGroupSupport { get; }

        bool SetReturnOnlyIDs(UGCQueryHandle_t handle, bool bReturnOnlyIDs);
        FunctionImplemented SetReturnOnlyIDsSupport { get; }

        bool SetReturnKeyValueTags(UGCQueryHandle_t handle, bool bReturnKeyValueTags);
        FunctionImplemented SetReturnKeyValueTagsSupport { get; }

        bool SetReturnMetadata(UGCQueryHandle_t handle, bool bReturnMetadata);
        FunctionImplemented SetReturnMetadataSupport { get; }

        bool SetReturnChildren(UGCQueryHandle_t handle, bool bReturnChildren);
        FunctionImplemented SetReturnChildrenSupport { get; }

        bool SetReturnAdditionalPreviews(UGCQueryHandle_t handle, bool bReturnAdditionalPreviews);
        FunctionImplemented SetReturnAdditionalPreviewsSupport { get; }

        bool SetReturnPlaytimeStats(UGCQueryHandle_t handle, UInt32 unDays);
        FunctionImplemented SetReturnPlaytimeStatsSupport { get; }

        bool SetLanguage(UGCQueryHandle_t handle, string pchLanguage);
        FunctionImplemented SetLanguageSupport { get; }

        bool SetAllowCachedResponse(UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds);
        FunctionImplemented SetAllowCachedResponseSupport { get; }

        bool SetTimeCreatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd);
        FunctionImplemented SetTimeCreatedDateRangeSupport { get; }

        bool SetTimeUpdatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd);
        FunctionImplemented SetTimeUpdatedDateRangeSupport { get; }

        bool AddRequiredKeyValueTag(UGCQueryHandle_t handle, string pKey, string pValue);
        FunctionImplemented AddRequiredKeyValueTagSupport { get; }

        bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, string pchLanguage);
        FunctionImplemented SetItemUpdateLanguageSupport { get; }

        bool SetAllowLegacyUpload(UGCUpdateHandle_t handle, bool bAllowLegacyUpload);
        FunctionImplemented SetAllowLegacyUploadSupport { get; }

        bool RemoveAllItemKeyValueTags(UGCUpdateHandle_t handle);
        FunctionImplemented RemoveAllItemKeyValueTagsSupport { get; }

        bool RemoveItemKeyValueTags(UGCUpdateHandle_t handle, string pchKey);
        FunctionImplemented RemoveItemKeyValueTagsSupport { get; }

        bool AddItemKeyValueTag(UGCUpdateHandle_t handle, string pchKey, string pchValue);
        FunctionImplemented AddItemKeyValueTagSupport { get; }

        bool AddItemPreviewFile(UGCUpdateHandle_t handle, string pszPreviewFile, EItemPreviewType type);
        FunctionImplemented AddItemPreviewFileSupport { get; }

        bool AddItemPreviewVideo(UGCUpdateHandle_t handle, string pszVideoID);
        FunctionImplemented AddItemPreviewVideoSupport { get; }

        bool UpdateItemPreviewFile(UGCUpdateHandle_t handle, UInt32 index, string pszPreviewFile);
        FunctionImplemented UpdateItemPreviewFileSupport { get; }

        bool UpdateItemPreviewVideo(UGCUpdateHandle_t handle, UInt32 index, string pszVideoID);
        FunctionImplemented UpdateItemPreviewVideoSupport { get; }

        bool RemoveItemPreview(UGCUpdateHandle_t handle, UInt32 index);
        FunctionImplemented RemoveItemPreviewSupport { get; }

        UInt64 SetUserItemVote(PublishedFileId_t nPublishedFileID, bool bVoteUp);
        FunctionImplemented SetUserItemVoteSupport { get; }

        UInt64 GetUserItemVote(PublishedFileId_t nPublishedFileID);
        FunctionImplemented GetUserItemVoteSupport { get; }

        bool BInitWorkshopForGameServer(UInt32 unWorkshopDepotID, string pszFolder);
        FunctionImplemented BInitWorkshopForGameServerSupport { get; }

        void SuspendDownloads(bool bSuspend);
        FunctionImplemented SuspendDownloadsSupport { get; }

        UInt64 StartPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        FunctionImplemented StartPlaytimeTrackingSupport { get; }

        UInt64 StopPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        FunctionImplemented StopPlaytimeTrackingSupport { get; }

        UInt64 StopPlaytimeTrackingForAllItems();
        FunctionImplemented StopPlaytimeTrackingForAllItemsSupport { get; }

        UInt64 AddDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID);
        FunctionImplemented AddDependencySupport { get; }

        UInt64 RemoveDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID);
        FunctionImplemented RemoveDependencySupport { get; }

        UInt64 AddAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID);
        FunctionImplemented AddAppDependencySupport { get; }

        UInt64 RemoveAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID);
        FunctionImplemented RemoveAppDependencySupport { get; }

        UInt64 GetAppDependencies(PublishedFileId_t nPublishedFileID);
        FunctionImplemented GetAppDependenciesSupport { get; }

        UInt64 DeleteItem(PublishedFileId_t nPublishedFileID);
        FunctionImplemented DeleteItemSupport { get; }

        bool ShowWorkshopEULA();
        FunctionImplemented ShowWorkshopEULASupport { get; }

        UInt64 GetWorkshopEULAStatus();
        FunctionImplemented GetWorkshopEULAStatusSupport { get; }
    }
}
