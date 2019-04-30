﻿using System;
using System.Runtime.InteropServices;

namespace SteamVent.SteamClient.Interfaces
{
    public enum ELobbyType : int
    {
        k_ELobbyTypePrivate = 0,
        k_ELobbyTypeFriendsOnly = 1,
        k_ELobbyTypePublic = 2,
        k_ELobbyTypeInvisible = 3,
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
        k_EChatAccountInstanceMask = 4095,
        k_EChatInstanceFlagClan = 524288,
        k_EChatInstanceFlagLobby = 262144,
        k_EChatInstanceFlagMMSLobby = 131072,
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
