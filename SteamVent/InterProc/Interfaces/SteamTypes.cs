using System;
using System.Runtime.InteropServices;

namespace SteamVent.InterProc.Interfaces
{
    public enum ELobbyType : int
    {
        k_ELobbyTypePrivate = 0,
        k_ELobbyTypeFriendsOnly = 1,
        k_ELobbyTypePublic = 2,
        k_ELobbyTypeInvisible = 3,
        k_ELobbyTypePrivateUnique = 4,
    };

    public enum ENotificationPosition : int
    {
        k_EPositionTopLeft = 0,
        k_EPositionTopRight = 1,
        k_EPositionBottomLeft = 2,
        k_EPositionBottomRight = 3,
    };

    public enum EChatMemberStateChange : int
    {
        k_EChatMemberStateChangeEntered = 1,
        k_EChatMemberStateChangeLeft = 2,
        k_EChatMemberStateChangeDisconnected = 4,
        k_EChatMemberStateChangeKicked = 8,
        k_EChatMemberStateChangeBanned = 16,
    };

    public enum EServerMode : int
    {
        eServerModeInvalid = 0,
        eServerModeNoAuthentication = 1,
        eServerModeAuthentication = 2,
        eServerModeAuthenticationAndSecure = 3,
    };

    public enum EUniverse : int
    {
        k_EUniverseInvalid = 0,
        k_EUniversePublic = 1,
        k_EUniverseBeta = 2,
        k_EUniverseInternal = 3,
        k_EUniverseDev = 4,
        k_EUniverseMax = 5,
    };

    public enum ShareType_t : int
    {
        SHARE_STOPIMMEDIATELY = 0,
        SHARE_RATIO = 1,
        SHARE_MANUAL = 2,
    };

    public enum EChatSteamIDInstanceFlags : int
    {
        k_EChatAccountInstanceMask  = 0x00FFF,
        k_EChatInstanceFlagClan     = 0x80000,
        k_EChatInstanceFlagLobby    = 0x40000,
        k_EChatInstanceFlagMMSLobby = 0x20000,
    };

    public enum EUGCQuery : int
    {
	    k_EUGCQuery_RankedByVote								  = 0,
	    k_EUGCQuery_RankedByPublicationDate						  = 1,
	    k_EUGCQuery_AcceptedForGameRankedByAcceptanceDate		  = 2,
	    k_EUGCQuery_RankedByTrend								  = 3,
	    k_EUGCQuery_FavoritedByFriendsRankedByPublicationDate	  = 4,
	    k_EUGCQuery_CreatedByFriendsRankedByPublicationDate		  = 5,
	    k_EUGCQuery_RankedByNumTimesReported					  = 6,
	    k_EUGCQuery_CreatedByFollowedUsersRankedByPublicationDate = 7,
	    k_EUGCQuery_NotYetRated									  = 8,
	    k_EUGCQuery_RankedByTotalVotesAsc						  = 9,
	    k_EUGCQuery_RankedByVotesUp								  = 10,
	    k_EUGCQuery_RankedByTextSearch							  = 11,
	    k_EUGCQuery_RankedByTotalUniqueSubscriptions			  = 12,
	    k_EUGCQuery_RankedByPlaytimeTrend						  = 13,
	    k_EUGCQuery_RankedByTotalPlaytime						  = 14,
	    k_EUGCQuery_RankedByAveragePlaytimeTrend				  = 15,
	    k_EUGCQuery_RankedByLifetimeAveragePlaytime				  = 16,
	    k_EUGCQuery_RankedByPlaytimeSessionsTrend				  = 17,
	    k_EUGCQuery_RankedByLifetimePlaytimeSessions			  = 18,
	    k_EUGCQuery_RankedByLastUpdatedDate						  = 19,
    };

    public enum EItemStatistic
	{
		k_EItemStatistic_NumSubscriptions					 = 0,
		k_EItemStatistic_NumFavorites						 = 1,
		k_EItemStatistic_NumFollowers						 = 2,
		k_EItemStatistic_NumUniqueSubscriptions				 = 3,
		k_EItemStatistic_NumUniqueFavorites					 = 4,
		k_EItemStatistic_NumUniqueFollowers					 = 5,
		k_EItemStatistic_NumUniqueWebsiteViews				 = 6,
		k_EItemStatistic_ReportScore						 = 7,
		k_EItemStatistic_NumSecondsPlayed					 = 8,
		k_EItemStatistic_NumPlaytimeSessions				 = 9,
		k_EItemStatistic_NumComments						 = 10,
		k_EItemStatistic_NumSecondsPlayedDuringTimePeriod	 = 11,
		k_EItemStatistic_NumPlaytimeSessionsDuringTimePeriod = 12,
	};

    public enum EUserUGCList : int
    {
        k_EUserUGCList_Published = 0,
        k_EUserUGCList_VotedOn = 1,
        k_EUserUGCList_VotedUp = 2,
        k_EUserUGCList_VotedDown = 3,
        k_EUserUGCList_WillVoteLater = 4,
        k_EUserUGCList_Favorited = 5,
        k_EUserUGCList_Subscribed = 6,
        k_EUserUGCList_UsedOrPlayed = 7,
        k_EUserUGCList_Followed = 8,
    };

    public enum EUGCMatchingUGCType : int
    {
	    k_EUGCMatchingUGCType_Items				 = 0,		// both mtx items and ready-to-use items
	    k_EUGCMatchingUGCType_Items_Mtx			 = 1,
	    k_EUGCMatchingUGCType_Items_ReadyToUse	 = 2,
	    k_EUGCMatchingUGCType_Collections		 = 3,
	    k_EUGCMatchingUGCType_Artwork			 = 4,
	    k_EUGCMatchingUGCType_Videos			 = 5,
	    k_EUGCMatchingUGCType_Screenshots		 = 6,
	    k_EUGCMatchingUGCType_AllGuides			 = 7,		// both web guides and integrated guides
	    k_EUGCMatchingUGCType_WebGuides			 = 8,
	    k_EUGCMatchingUGCType_IntegratedGuides	 = 9,
	    k_EUGCMatchingUGCType_UsableInGame		 = 10,		// ready-to-use items and integrated guides
	    k_EUGCMatchingUGCType_ControllerBindings = 11,
	    k_EUGCMatchingUGCType_GameManagedItems	 = 12,		// game managed items (not managed by users)
	    k_EUGCMatchingUGCType_All				 = ~0,		// (only be valid for CreateQueryUserUGCRequest requests)
    };

    public enum EUserUGCListSortOrder : int
    {
        k_EUserUGCListSortOrder_CreationOrderDesc = 0,
        k_EUserUGCListSortOrder_CreationOrderAsc = 1,
        k_EUserUGCListSortOrder_TitleAsc = 2,
        k_EUserUGCListSortOrder_LastUpdatedDesc = 3,
        k_EUserUGCListSortOrder_SubscriptionDateDesc = 4,
        k_EUserUGCListSortOrder_VoteScoreDesc = 5,
        k_EUserUGCListSortOrder_ForModeration = 6,
    };

    public enum EWorkshopFileType : int
    {
	    k_EWorkshopFileTypeFirst = 0,

	    k_EWorkshopFileTypeCommunity			  = 0,		// normal Workshop item that can be subscribed to
	    k_EWorkshopFileTypeMicrotransaction		  = 1,		// Workshop item that is meant to be voted on for the purpose of selling in-game
	    k_EWorkshopFileTypeCollection			  = 2,		// a collection of Workshop or Greenlight items
	    k_EWorkshopFileTypeArt					  = 3,		// artwork
	    k_EWorkshopFileTypeVideo				  = 4,		// external video
	    k_EWorkshopFileTypeScreenshot			  = 5,		// screenshot
	    k_EWorkshopFileTypeGame					  = 6,		// Greenlight game entry
	    k_EWorkshopFileTypeSoftware				  = 7,		// Greenlight software entry
	    k_EWorkshopFileTypeConcept				  = 8,		// Greenlight concept
	    k_EWorkshopFileTypeWebGuide				  = 9,		// Steam web guide
	    k_EWorkshopFileTypeIntegratedGuide		  = 10,		// application integrated guide
	    k_EWorkshopFileTypeMerch				  = 11,		// Workshop merchandise meant to be voted on for the purpose of being sold
	    k_EWorkshopFileTypeControllerBinding	  = 12,		// Steam Controller bindings
	    k_EWorkshopFileTypeSteamworksAccessInvite = 13,		// internal
	    k_EWorkshopFileTypeSteamVideo			  = 14,		// Steam video
	    k_EWorkshopFileTypeGameManagedItem		  = 15,		// managed completely by the game, not the user, and not shown on the web
	    k_EWorkshopFileTypeClip					  = 16,		// internal
	    k_EWorkshopFileTypeMax = 17
    };

    public enum EItemUpdateStatus
    {
	    k_EItemUpdateStatusInvalid 				= 0, // The item update handle was invalid, job might be finished, listen too SubmitItemUpdateResult_t
	    k_EItemUpdateStatusPreparingConfig 		= 1, // The item update is processing configuration data
	    k_EItemUpdateStatusPreparingContent		= 2, // The item update is reading and processing content files
	    k_EItemUpdateStatusUploadingContent		= 3, // The item update is uploading content changes to Steam
	    k_EItemUpdateStatusUploadingPreviewFile	= 4, // The item update is uploading new preview file image
	    k_EItemUpdateStatusCommittingChanges	= 5  // The item update is committing all changes
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SteamParamStringArray_t
    {
        public IntPtr m_ppStrings;
        public Int32 m_nNumStrings;
    };

    public enum EResult : int
    {
	    k_EResultNone = 0,							// no result
	    k_EResultOK	= 1,							// success
	    k_EResultFail = 2,							// generic failure 
	    k_EResultNoConnection = 3,					// no/failed network connection
        //k_EResultNoConnectionRetry = 4,			// OBSOLETE - removed
	    k_EResultInvalidPassword = 5,				// password/ticket is invalid
	    k_EResultLoggedInElsewhere = 6,				// same user logged in elsewhere
	    k_EResultInvalidProtocolVer = 7,			// protocol version is incorrect
	    k_EResultInvalidParam = 8,					// a parameter is incorrect
	    k_EResultFileNotFound = 9,					// file was not found
	    k_EResultBusy = 10,							// called method busy - action not taken
	    k_EResultInvalidState = 11,					// called object was in an invalid state
	    k_EResultInvalidName = 12,					// name is invalid
	    k_EResultInvalidEmail = 13,					// email is invalid
	    k_EResultDuplicateName = 14,				// name is not unique
	    k_EResultAccessDenied = 15,					// access is denied
	    k_EResultTimeout = 16,						// operation timed out
	    k_EResultBanned = 17,						// VAC2 banned
	    k_EResultAccountNotFound = 18,				// account not found
	    k_EResultInvalidSteamID = 19,				// steamID is invalid
	    k_EResultServiceUnavailable = 20,			// The requested service is currently unavailable
	    k_EResultNotLoggedOn = 21,					// The user is not logged on
	    k_EResultPending = 22,						// Request is pending (may be in process, or waiting on third party)
	    k_EResultEncryptionFailure = 23,			// Encryption or Decryption failed
	    k_EResultInsufficientPrivilege = 24,		// Insufficient privilege
	    k_EResultLimitExceeded = 25,				// Too much of a good thing
	    k_EResultRevoked = 26,						// Access has been revoked (used for revoked guest passes)
	    k_EResultExpired = 27,						// License/Guest pass the user is trying to access is expired
	    k_EResultAlreadyRedeemed = 28,				// Guest pass has already been redeemed by account, cannot be acked again
	    k_EResultDuplicateRequest = 29,				// The request is a duplicate and the action has already occurred in the past, ignored this time
	    k_EResultAlreadyOwned = 30,					// All the games in this guest pass redemption request are already owned by the user
	    k_EResultIPNotFound = 31,					// IP address not found
	    k_EResultPersistFailed = 32,				// failed to write change to the data store
	    k_EResultLockingFailed = 33,				// failed to acquire access lock for this operation
	    k_EResultLogonSessionReplaced = 34,
	    k_EResultConnectFailed = 35,
	    k_EResultHandshakeFailed = 36,
	    k_EResultIOFailure = 37,
	    k_EResultRemoteDisconnect = 38,
	    k_EResultShoppingCartNotFound = 39,			// failed to find the shopping cart requested
	    k_EResultBlocked = 40,						// a user didn't allow it
	    k_EResultIgnored = 41,						// target is ignoring sender
	    k_EResultNoMatch = 42,						// nothing matching the request found
	    k_EResultAccountDisabled = 43,
	    k_EResultServiceReadOnly = 44,				// this service is not accepting content changes right now
	    k_EResultAccountNotFeatured = 45,			// account doesn't have value, so this feature isn't available
	    k_EResultAdministratorOK = 46,				// allowed to take this action, but only because requester is admin
	    k_EResultContentVersion = 47,				// A Version mismatch in content transmitted within the Steam protocol.
	    k_EResultTryAnotherCM = 48,					// The current CM can't service the user making a request, user should try another.
	    k_EResultPasswordRequiredToKickSession = 49,// You are already logged in elsewhere, this cached credential login has failed.
	    k_EResultAlreadyLoggedInElsewhere = 50,		// You are already logged in elsewhere, you must wait
	    k_EResultSuspended = 51,					// Long running operation (content download) suspended/paused
	    k_EResultCancelled = 52,					// Operation canceled (typically by user: content download)
	    k_EResultDataCorruption = 53,				// Operation canceled because data is ill formed or unrecoverable
	    k_EResultDiskFull = 54,						// Operation canceled - not enough disk space.
	    k_EResultRemoteCallFailed = 55,				// an remote call or IPC call failed
	    k_EResultPasswordUnset = 56,				// Password could not be verified as it's unset server side
	    k_EResultExternalAccountUnlinked = 57,		// External account (PSN, Facebook...) is not linked to a Steam account
	    k_EResultPSNTicketInvalid = 58,				// PSN ticket was invalid
	    k_EResultExternalAccountAlreadyLinked = 59,	// External account (PSN, Facebook...) is already linked to some other account, must explicitly request to replace/delete the link first
	    k_EResultRemoteFileConflict = 60,			// The sync cannot resume due to a conflict between the local and remote files
	    k_EResultIllegalPassword = 61,				// The requested new password is not legal
	    k_EResultSameAsPreviousValue = 62,			// new value is the same as the old one ( secret question and answer )
	    k_EResultAccountLogonDenied = 63,			// account login denied due to 2nd factor authentication failure
	    k_EResultCannotUseOldPassword = 64,			// The requested new password is not legal
	    k_EResultInvalidLoginAuthCode = 65,			// account login denied due to auth code invalid
	    k_EResultAccountLogonDeniedNoMail = 66,		// account login denied due to 2nd factor auth failure - and no mail has been sent - partner site specific
	    k_EResultHardwareNotCapableOfIPT = 67,		// 
	    k_EResultIPTInitError = 68,					// 
	    k_EResultParentalControlRestricted = 69,	// operation failed due to parental control restrictions for current user
	    k_EResultFacebookQueryError = 70,			// Facebook query returned an error
	    k_EResultExpiredLoginAuthCode = 71,			// account login denied due to auth code expired
	    k_EResultIPLoginRestrictionFailed = 72,
	    k_EResultAccountLockedDown = 73,
	    k_EResultAccountLogonDeniedVerifiedEmailRequired = 74,
	    k_EResultNoMatchingURL = 75,
	    k_EResultBadResponse = 76,					// parse failure, missing field, etc.
	    k_EResultRequirePasswordReEntry = 77,		// The user cannot complete the action until they re-enter their password
	    k_EResultValueOutOfRange = 78,				// the value entered is outside the acceptable range
	    k_EResultUnexpectedError = 79,				// something happened that we didn't expect to ever happen
	    k_EResultDisabled = 80,						// The requested service has been configured to be unavailable
	    k_EResultInvalidCEGSubmission = 81,			// The set of files submitted to the CEG server are not valid !
	    k_EResultRestrictedDevice = 82,				// The device being used is not allowed to perform this action
	    k_EResultRegionLocked = 83,					// The action could not be complete because it is region restricted
	    k_EResultRateLimitExceeded = 84,			// Temporary rate limit exceeded, try again later, different from k_EResultLimitExceeded which may be permanent
	    k_EResultAccountLoginDeniedNeedTwoFactor = 85,	// Need two-factor code to login
	    k_EResultItemDeleted = 86,					// The thing we're trying to access has been deleted
	    k_EResultAccountLoginDeniedThrottle = 87,	// login attempt failed, try to throttle response to possible attacker
	    k_EResultTwoFactorCodeMismatch = 88,		// two factor code mismatch
	    k_EResultTwoFactorActivationCodeMismatch = 89,	// activation code for two-factor didn't match
	    k_EResultAccountAssociatedToMultiplePartners = 90,	// account has been associated with multiple partners
	    k_EResultNotModified = 91,					// data not modified
	    k_EResultNoMobileDevice = 92,				// the account does not have a mobile device associated with it
	    k_EResultTimeNotSynced = 93,				// the time presented is out of range or tolerance
	    k_EResultSmsCodeFailed = 94,				// SMS code failure (no match, none pending, etc.)
	    k_EResultAccountLimitExceeded = 95,			// Too many accounts access this resource
	    k_EResultAccountActivityLimitExceeded = 96,	// Too many changes to this account
	    k_EResultPhoneActivityLimitExceeded = 97,	// Too many changes to this phone
	    k_EResultRefundToWallet = 98,				// Cannot refund to payment method, must use wallet
	    k_EResultEmailSendFailure = 99,				// Cannot send an email
	    k_EResultNotSettled = 100,					// Can't perform operation till payment has settled
	    k_EResultNeedCaptcha = 101,					// Needs to provide a valid captcha
	    k_EResultGSLTDenied = 102,					// a game server login token owned by this token's owner has been banned
	    k_EResultGSOwnerDenied = 103,				// game server owner is denied for other reason (account lock, community ban, vac ban, missing phone)
	    k_EResultInvalidItemType = 104,				// the type of thing we were requested to act on is invalid
	    k_EResultIPBanned = 105,					// the ip address has been banned from taking this action
	    k_EResultGSLTExpired = 106,					// this token has expired from disuse; can be reset for use
	    k_EResultInsufficientFunds = 107,			// user doesn't have enough wallet funds to complete the action
	    k_EResultTooManyPending = 108,				// There are too many of this thing pending already
	    k_EResultNoSiteLicensesFound = 109,			// No site licenses found
	    k_EResultWGNetworkSendExceeded = 110,		// the WG couldn't send a response because we exceeded max network send size
	    k_EResultAccountNotFriends = 111,			// the user is not mutually friends
	    k_EResultLimitedUserAccount = 112,			// the user is limited
	    k_EResultCantRemoveItem = 113,				// item can't be removed
	    k_EResultAccountDeleted = 114,				// account has been deleted
	    k_EResultExistingUserCancelledLicense = 115,	// A license for this already exists, but cancelled
	    k_EResultCommunityCooldown = 116,			// access is denied because of a community cooldown (probably from support profile data resets)
	    k_EResultNoLauncherSpecified = 117,			// No launcher was specified, but a launcher was needed to choose correct realm for operation.
	    k_EResultMustAgreeToSSA = 118,				// User must agree to china SSA or global SSA before login
	    k_EResultLauncherMigrated = 119,			// The specified launcher type is no longer supported; the user should be directed elsewhere
	    k_EResultSteamRealmMismatch = 120,			// The user's realm does not match the realm of the requested resource
	    k_EResultInvalidSignature = 121,			// signature check did not match
	    k_EResultParseFailure = 122,				// Failed to parse input
	    k_EResultNoVerifiedPhone = 123,				// account does not have a verified phone number
	    k_EResultInsufficientBattery = 124,			// user device doesn't have enough battery charge currently to complete the action
	    k_EResultChargerRequired = 125,				// The operation requires a charger to be plugged in, which wasn't present
	    k_EResultCachedCredentialInvalid = 126,		// Cached credential was invalid - user must reauthenticate
	    K_EResultPhoneNumberIsVOIP = 127,			// The phone number provided is a Voice Over IP number
	    k_EResultNotSupported = 128,				// The data being accessed is not supported by this API
	    k_EResultFamilySizeLimitExceeded = 129,		// Reached the maximum size of the family
    };
    
    public enum ERemoteStoragePublishedFileVisibility : int
    {
        k_ERemoteStoragePublishedFileVisibilityPublic = 0,
        k_ERemoteStoragePublishedFileVisibilityFriendsOnly = 1,
        k_ERemoteStoragePublishedFileVisibilityPrivate = 2,
        k_ERemoteStoragePublishedFileVisibilityUnlisted = 3,
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SteamUGCDetails_t
    {
        public UInt64 m_nPublishedFileId;
        public EResult m_eResult;
        public EWorkshopFileType m_eFileType;
        public UInt32 m_nCreatorAppID;
        public UInt32 m_nConsumerAppID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
        public string m_rgchTitle;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8000)]
        public string m_rgchDescription;
        public UInt64 m_ulSteamIDOwner;
        public UInt32 m_rtimeCreated;
        public UInt32 m_rtimeUpdated;
        public UInt32 m_rtimeAddedToUserList;
        public ERemoteStoragePublishedFileVisibility m_eVisibility;
        [MarshalAs(UnmanagedType.I1)]
        public bool m_bBanned;
        [MarshalAs(UnmanagedType.I1)]
        public bool m_bAcceptedForUse;
        [MarshalAs(UnmanagedType.I1)]
        public bool m_bTagsTruncated;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1025)]
        public string m_rgchTags;
        public UInt64 m_hFile;
        public UInt64 m_hPreviewFile;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string m_pchFileName;
        public Int32 m_nFileSize;
        public Int32 m_nPreviewFileSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string m_rgchURL;
        public UInt32 m_unVotesUp;
        public UInt32 m_unVotesDown;
        public float m_flScore;
        public UInt32 m_unNumChildren;
    };

    public enum ECallbackType : int
    {
        k_iSteamUserCallbacks = 100,
        k_iSteamGameServerCallbacks = 200,
        k_iSteamFriendsCallbacks = 300,
        k_iSteamBillingCallbacks = 400,
        k_iSteamMatchmakingCallbacks = 500,
        k_iSteamContentServerCallbacks = 600,
        k_iSteamUtilsCallbacks = 700,
        k_iClientFriendsCallbacks = 800,
        k_iClientUserCallbacks = 900,
        k_iSteamAppsCallbacks = 1000,
        k_iSteamUserStatsCallbacks = 1100,
        k_iSteamNetworkingCallbacks = 1200,
        k_iClientRemoteStorageCallbacks = 1300,
        k_iSteamUserItemsCallbacks = 1400,
        k_iSteamGameServerItemsCallbacks = 1500,
        k_iClientUtilsCallbacks = 1600,
        k_iSteamGameCoordinatorCallbacks = 1700,
        k_iSteamGameServerStatsCallbacks = 1800,
        k_iSteam2AsyncCallbacks = 1900,
        k_iSteamGameStatsCallbacks = 2000,
        k_iClientHTTPCallbacks = 2100,
        k_iClientScreenshotsCallbacks = 2200,
        k_iSteamScreenshotsCallbacks = 2300,
        k_iClientAudioCallbacks = 2400,
        k_iSteamUnifiedMessagesCallbacks = 2500,
        k_iClientUnifiedMessagesCallbacks = 2600,
        k_iClientControllerCallbacks = 2700,
        k_iSteamControllerCallbacks = 2800,
        k_iClientParentalSettingsCallbacks = 2900,
        k_iClientDeviceAuthCallbacks = 3000,
        k_iClientNetworkDeviceManagerCallbacks = 3100,
        k_iClientMusicCallbacks = 3200,
        k_iClientRemoteClientManagerCallbacks = 3300,
        k_iClientUGCCallbacks = 3400,
        k_iSteamStreamClientCallbacks = 3500,
        k_IClientProductBuilderCallbacks = 3600,
        k_iClientShortcutsCallbacks = 3700,
        k_iClientRemoteControlManagerCallbacks = 3800,
        k_iSteamAppListCallbacks = 3900,
        k_iSteamMusicCallbacks = 4000,
        k_iSteamMusicRemoteCallbacks = 4100,
        k_iClientVRCallbacks = 4200,
        k_iClientReservedCallbacks = 4300,
        k_iSteamReservedCallbacks = 4400,
        k_iSteamHTMLSurfaceCallbacks = 4500,
        k_iClientVideoCallbacks = 4600,
        k_iClientInventoryCallbacks = 4700,
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct CallbackMsg_t
    {
        public Int32 m_hSteamUser;
        public Int32 m_iCallback;
        public IntPtr m_pubParam;
        public Int32 m_cubParam;
    };
}
