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
    public interface ISteamUGC
    {
        UInt64 CreateQueryUserUGCRequest(UInt32 unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
        UInt64 CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, UInt32 nCreatorAppID, UInt32 nConsumerAppID, UInt32 unPage);
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
        // AddItemToFavorites
        // RemoteItemFromFavorites
        UInt64 SubscribeItem(UInt64 nPublishedFileID);
        UInt64 UnsubscribeItem(UInt64 nPublishedFileID);
        UInt32 GetNumSubscribedItems();
        UInt32 GetSubscribedItems(ref UInt64 pvecPublishedFileID, UInt32 cMaxEntries);
        //GetItemState
        bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize);
        bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref bool pbLegacyItem);
        bool GetItemInstallInfo(UInt64 nPublishedFileID, ref UInt64 punSizeOnDisk, StringBuilder pchFolder, UInt32 cchFolderSize, ref UInt32 punTimeStamp);
        bool GetItemUpdateInfo(UInt64 nPublishedFileID, ref bool pbNeedsUpdate, ref bool pbIsDownloading, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);
        bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, ref UInt64 punBytesDownloaded, ref UInt64 punBytesTotal);

        //DownloadItem
    }
}
