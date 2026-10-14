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
        UInt64 CreateQueryUserUGCRequest(UInt32 unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
        UInt64 CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
        // Deep-paging overload (STEAMUGC_INTERFACE_VERSION012+): pchCursor is null or "*" for the first page.
        UInt64 CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, string pchCursor);
        UInt64 CreateQueryUGCDetailsRequest(ref PublishedFileId_t pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        UInt64 SendQueryUGCRequest(UGCQueryHandle_t handle);
        bool GetQueryUGCResult(UGCQueryHandle_t handle, UInt32 index, ref SteamUGCDetails_t pDetails);
        bool GetQueryUGCPreviewURL(UGCQueryHandle_t handle, UInt32 index, ref string pchURL, UInt32 cchURLSize);
        bool GetQueryUGCMetadata(UGCQueryHandle_t handle, UInt32 index, ref string pchMetadata, UInt32 cchMetadatasize);
        bool GetQueryUGCChildren(UGCQueryHandle_t handle, UInt32 index, ref PublishedFileId_t pvecPublishedFileID, UInt32 cMaxEntries);
        bool GetQueryUGCStatistic(UGCQueryHandle_t handle, UInt32 index, EItemStatistic eStatType, ref UInt32 pStatValue);
        UInt32 GetQueryUGCNumAdditionalPreviews(UGCQueryHandle_t handle, UInt32 index);
        bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, UInt32 index, UInt32 previewIndex, ref string pchURLOrVideoID, UInt32 cchURLSize, ref bool pbIsImage);


        bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle);
        bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName);
        bool AddExcludedTag(UGCQueryHandle_t handle, string pTagName);
        bool SetReturnLongDescription(UGCQueryHandle_t handle, bool bReturnLongDescription);
        bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool bReturnTotalOnly);
        bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName);
        bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag);
        bool SetSearchText(UGCQueryHandle_t handle, string pSearchText);
        bool SetRankedByTrendDays(UGCQueryHandle_t handle, UInt32 unDays);
        UInt64 RequestUGCDetails(UInt64 nPublishedFileID);
        UInt64 RequestUGCDetails(UInt64 nPublishedFileID, UInt32 unMaxAgeSeconds);


        UInt64 CreateItem(UInt32 nConsumerAppId, EWorkshopFileType eFileType);
        UInt64 StartItemUpdate(UInt32 nConsumerAppId, UInt64 nPublishedFileID);
        bool SetItemTitle(UInt64 handle, string pchTitle);
        bool SetItemDescription(UInt64 handle, string pchDescription);
        bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData);
        bool SetItemVisibility(UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility);
        bool SetItemTags(UGCUpdateHandle_t updateHandle, ref SteamParamStringArray_t pTags);
        bool SetItemContent(UGCUpdateHandle_t handle, string pszContentFolder);
        bool SetItemPreview(UGCUpdateHandle_t handle, string pszPreviewFile);
        UInt64 SubmitItemUpdate(UGCUpdateHandle_t handle, string pchChangeNote);
        EItemUpdateStatus GetItemUpdateProgress(UGCUpdateHandle_t handle, ref UInt64 punBytesProcessed, ref UInt64 punBytesTotal);
        UInt64 AddItemToFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID);
        UInt64 RemoveItemFromFavorites(UInt32 nCreatorAppID, UInt64 nPublishedFileID);
        UInt64 SubscribeItem(UInt64 nPublishedFileID);
        UInt64 UnsubscribeItem(UInt64 nPublishedFileID);
        UInt32 GetNumSubscribedItems();
        UInt32 GetSubscribedItems(UInt64[] pvecPublishedFileID, UInt32 cMaxEntries);
        EItemState GetItemState(UInt64 nPublishedFileID);
        bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize);
        bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref bool pbLegacyItem);
        bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp);
        bool GetItemUpdateInfo(UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);
        bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);

        bool DownloadItem(UInt64 nPublishedFileID, bool bHighPriority);
        UInt32 GetQueryUGCNumTags(UGCQueryHandle_t handle, UInt32 index);
        bool GetQueryUGCTag(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize);
        bool GetQueryUGCTagDisplayName(UGCQueryHandle_t handle, UInt32 index, UInt32 indexTag, StringBuilder pchValue, UInt32 cchValueSize);
        UInt32 GetQueryUGCNumKeyValueTags(UGCQueryHandle_t handle, UInt32 index);
        bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, UInt32 keyValueTagIndex, StringBuilder pchKey, UInt32 cchKeySize, StringBuilder pchValue, UInt32 cchValueSize);
        bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, UInt32 index, string pchKey, StringBuilder pchValue, UInt32 cchValueSize);
        bool AddRequiredTagGroup(UGCQueryHandle_t handle, ref SteamParamStringArray_t pTagGroups);
        bool SetReturnOnlyIDs(UGCQueryHandle_t handle, bool bReturnOnlyIDs);
        bool SetReturnKeyValueTags(UGCQueryHandle_t handle, bool bReturnKeyValueTags);
        bool SetReturnMetadata(UGCQueryHandle_t handle, bool bReturnMetadata);
        bool SetReturnChildren(UGCQueryHandle_t handle, bool bReturnChildren);
        bool SetReturnAdditionalPreviews(UGCQueryHandle_t handle, bool bReturnAdditionalPreviews);
        bool SetReturnPlaytimeStats(UGCQueryHandle_t handle, UInt32 unDays);
        bool SetLanguage(UGCQueryHandle_t handle, string pchLanguage);
        bool SetAllowCachedResponse(UGCQueryHandle_t handle, UInt32 unMaxAgeSeconds);
        bool SetTimeCreatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd);
        bool SetTimeUpdatedDateRange(UGCQueryHandle_t handle, UInt32 rtStart, UInt32 rtEnd);
        bool AddRequiredKeyValueTag(UGCQueryHandle_t handle, string pKey, string pValue);
        bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, string pchLanguage);
        bool SetAllowLegacyUpload(UGCUpdateHandle_t handle, bool bAllowLegacyUpload);
        bool RemoveAllItemKeyValueTags(UGCUpdateHandle_t handle);
        bool RemoveItemKeyValueTags(UGCUpdateHandle_t handle, string pchKey);
        bool AddItemKeyValueTag(UGCUpdateHandle_t handle, string pchKey, string pchValue);
        bool AddItemPreviewFile(UGCUpdateHandle_t handle, string pszPreviewFile, EItemPreviewType type);
        bool AddItemPreviewVideo(UGCUpdateHandle_t handle, string pszVideoID);
        bool UpdateItemPreviewFile(UGCUpdateHandle_t handle, UInt32 index, string pszPreviewFile);
        bool UpdateItemPreviewVideo(UGCUpdateHandle_t handle, UInt32 index, string pszVideoID);
        bool RemoveItemPreview(UGCUpdateHandle_t handle, UInt32 index);
        UInt64 SetUserItemVote(PublishedFileId_t nPublishedFileID, bool bVoteUp);
        UInt64 GetUserItemVote(PublishedFileId_t nPublishedFileID);
        bool BInitWorkshopForGameServer(UInt32 unWorkshopDepotID, string pszFolder);
        void SuspendDownloads(bool bSuspend);
        UInt64 StartPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        UInt64 StopPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, UInt32 unNumPublishedFileIDs);
        UInt64 StopPlaytimeTrackingForAllItems();
        UInt64 AddDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID);
        UInt64 RemoveDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID);
        UInt64 AddAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID);
        UInt64 RemoveAppDependency(PublishedFileId_t nPublishedFileID, UInt32 nAppID);
        UInt64 GetAppDependencies(PublishedFileId_t nPublishedFileID);
        UInt64 DeleteItem(PublishedFileId_t nPublishedFileID);
        bool ShowWorkshopEULA();
        UInt64 GetWorkshopEULAStatus();
    }
}
