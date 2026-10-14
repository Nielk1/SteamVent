# Steamworks VTable Reference

Authoritative per-version layouts for the interface families this library exposes — `ISteamApps`, `ISteamClient`, `ISteamUGC`, and `ISteamUtils`. Every `[VTableIndex]` in the generated `ISteam*NNN.cs` classes (`SteamVent/Steamworks/Native/Interfaces/`) is checked against the official Steamworks SDK (`references/steamworks_sdk`, git-tagged per SDK release — the tag that first carries each interface version supplies that version's `isteam*.h`). The only snapshots the official repo does not tag (`ISteamClient013`, `ISteamUGC004`/`006`, `ISteamUtils001`/`003`) are taken from the Goldberg Emulator SDK (`references/goldberg_emulator/sdk_includes/isteam*NNN.h`), the sole source for those.

## Legend

- A plain number is the **0-based vtable index** of that function in that version, and it is **mapped** (a real P/Invoke delegate) in the corresponding C# `ISteam*NNN` class.
- `*` — the function exists in that version's native vtable but is **not part of the C# interface** (unmapped public API).
- `†` — a **`STEAM_PRIVATE_API` / `DEPRECATED`** slot. It occupies a vtable index (so it shifts the indices of every function after it) but is **not part of the public C# surface**; it is listed so the index accounting stays transparent.
- `—` — the function does not exist in that version's native vtable.
- Where an interface declares two overloads occupying two slots (e.g. `ISteamUGC::CreateQueryAllUGCRequest`, `ISteamUGC::GetQueryUGCKeyValueTag`), each overload is its **own row** so every cell holds a single index.

## Counting rules (the source of most index-mapping errors)

- Indexes are **0-based** and follow declaration order in `isteam*.h`.
- **`#ifdef _PS3`** members are **excluded** — they are not part of the Windows vtable the C# wrapper targets.
- **`STEAM_PRIVATE_API(x)`** expands to `protected: x public:`, so those methods **do** occupy a slot and **are** counted (marked `†`).
- `DEPRECATED_*` helpers likewise occupy slots and are counted.
- These classic interfaces have **no leading version-string getter slot** — slot 0 is the first declared virtual function (e.g. `ISteamApps::BIsSubscribed`, `ISteamClient::CreateSteamPipe`, `ISteamUtils::GetSecondsSinceAppActive`).

> Because Steamworks inserts new functions **into the middle** of these vtables between versions, the same method resolves to a different index in each version — each generated class therefore carries its own `[VTableIndex]` set. Use the class whose interface-version string matches the one you requested.

---

## Interface version → Steamworks SDK tag

The Steamworks tag that first introduces each interface version. `—` (Goldberg) means the official repo has no such tag and the layout is sourced from Goldberg Emulator.

### `ISteamApps` (`STEAMAPPS_INTERFACE_VERSIONNNN`)
| Version | 001 | 002 | 003 | 004 | 005 | 006 | 007 | 008 | 009 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --- | 1.00 | 1.01 | 1.04 | 1.12 | 1.18 | 1.26a | 1.32 | 1.37 | 1.64–1.65* |

> * `009` was introduced in **1.64** (33 slots, ending at `SetActiveBeta`) and completed in **1.65** (35 slots, adding `SetGamePerformanceSetting`/`SetGameRenderResolution`); the C# `ISteamApps009` bindings target the **1.65** layout.

### `ISteamClient` (`SteamClientNNN`)
| Version | 007 | 008 | 009 | 010 | 011 | 012 | 013 | 014 | 015 | 016 | 017 | 018 | 019 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --- | 1.00 | 1.04 | 1.08 | 1.12 | 1.16 | 1.17 | — (Goldberg) | 1.29a | 1.30 | 1.31 | 1.32 | 1.43 | 1.46 |

### `ISteamUGC` (`STEAMUGC_INTERFACE_VERSIONNNN`)
| Version | 001 | 002 | 003 | 004 | 005 | 006 | 007 | 008 | 009 | 010 | 011 | 012 | 013 | 014 | 015 | 016 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --- | 1.26a | 1.29a | 1.31 | — (Goldberg) | 1.33b | — (Goldberg) | 1.34 | 1.37 | 1.38a | 1.40 | — (Goldberg) | 1.43 | 1.45 | 1.47 | 1.51 | 1.53a |

### `ISteamUtils` (`SteamUtilsNNN`)
| Version | 001 | 002 | 003 | 004 | 005 | 006 | 007 | 008 | 009 | 010 | 011 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --- | — (Goldberg) | 1.00 | — (Goldberg) | 1.04 | 1.07 | 1.25 | 1.29a | 1.37 | 1.40 | 1.50 | 1.65 |

> **C# scope:** `ISteamApps001–009`, `ISteamClient007–019`, `ISteamUGC001–016` (no `011`), and `ISteamUtils007` only. Versions beyond these are not implemented by the library.

---

## ISteamClient (entry-point client)

Interface-version strings: `SteamClient007` … `SteamClient019`. The contract exposes four `GetISteam*` factory slots plus `CreateSteamPipe` / `BReleaseSteamPipe` / `ConnectToGlobalUser` / `CreateLocalUser` / `ReleaseUser` and the lifecycle helpers `SetLocalIPBinding` / `RunFrame` / `SetWarningMessageHook` (backed by the `SteamAPIWarningMessageHook_t` delegate) / `BShutdownIfAllPipesClosed`. Steam interleaves *other* `GetISteam*` accessors (Friends, Matchmaking, RemoteStorage, HTTP, Controller, Input, …) between the slots this library exposes, so each exposed slot's index varies by version:

| Method | 007 | 008 | 009 | 010 | 011 | 012 | 013 | 014 | 015 | 016 | 017 | 018 | 019 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `CreateSteamPipe` | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `BReleaseSteamPipe` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| `ConnectToGlobalUser` | 2 | 2 | 2 | 2 | 2 | 2 | 2 | 2 | 2 | 2 | 2 | 2 | 2 |
| `CreateLocalUser` | 3 | 3 | 3 | 3 | 3 | 3 | 3 | 3 | 3 | 3 | 3 | 3 | 3 |
| `ReleaseUser` | 4 | 4 | 4 | 4 | 4 | 4 | 4 | 4 | 4 | 4 | 4 | 4 | 4 |
| `GetISteamUser` | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 |
| `GetISteamGameServer*` | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 |
| `SetLocalIPBinding` | 7 | 7 | 7 | 7 | 7 | 7 | 7 | 7 | 7 | 7 | 7 | 7 | 7 |
| `GetISteamFriends*` | 8 | 8 | 8 | 8 | 8 | 8 | 8 | 8 | 8 | 8 | 8 | 8 | 8 |
| `GetISteamUtils` | 9 | 9 | 9 | 9 | 9 | 9 | 9 | 9 | 9 | 9 | 9 | 9 | 9 |
| `GetISteamMatchmaking*` | 10 | 10 | 10 | 10 | 10 | 10 | 10 | 10 | 10 | 10 | 10 | 10 | 10 |
| `GetISteamContentServer*` | 11 | — | — | — | — | — | — | — | — | — | — | — | — |
| `GetISteamMasterServerUpdater*` | 12 | 11 | 11 | 11 | 11 | — | — | — | — | — | — | — | — |
| `GetISteamMatchmakingServers*` | 13 | 12 | 12 | 12 | 12 | 11 | 11 | 11 | 11 | 11 | 11 | 11 | 11 |
| `GetISteamGenericInterface*` | 14 | 13 | 13 | 13 | 13 | 12 | 12 | 12 | 12 | 12 | 12 | 12 | 12 |
| `GetISteamUserStats*` | 17 | 14 | 14 | 14 | 14 | 13 | 13 | 13 | 13 | 13 | 13 | 13 | 13 |
| `GetISteamGameServerStats*` | — | — | 15 | 15 | 15 | 14 | 14 | 14 | 14 | 14 | 14 | 14 | 14 |
| `GetISteamApps` | 18 | 15 | 16 | 16 | 16 | 15 | 15 | 15 | 15 | 15 | 15 | 15 | 15 |
| `GetISteamNetworking*` | 19 | 16 | 17 | 17 | 17 | 16 | 16 | 16 | 16 | 16 | 16 | 16 | 16 |
| `GetISteamRemoteStorage*` | — | 17 | 18 | 18 | 18 | 17 | 17 | 17 | 17 | 17 | 17 | 17 | 17 |
| `GetISteamScreenshots*` | — | — | — | — | 19 | 18 | 18 | 18 | 18 | 18 | 18 | 18 | 18 |
| `GetISteamGameSearch*` | — | — | — | — | — | — | — | — | — | — | — | 19 | 19 |
| `RunFrame†` | 15 | 18 | 19 | 19 | 20 | 19 | 19 | 19 | 19 | 19 | 19 | 20 | 20 |
| `GetIPCCallCount*` | 16 | 19 | 20 | 20 | 21 | 20 | 20 | 20 | 20 | 20 | 20 | 21 | 21 |
| `SetWarningMessageHook` | 20 | 20 | 21 | 21 | 22 | 21 | 21 | 21 | 21 | 21 | 21 | 22 | 22 |
| `BShutdownIfAllPipesClosed` | — | — | — | 22 | 23 | 22 | 22 | 22 | 22 | 22 | 22 | 23 | 23 |
| `GetISteamHTTP*` | — | — | — | — | 24 | 23 | 23 | 23 | 23 | 23 | 23 | 24 | 24 |
| `GetISteamUnifiedMessages*` | — | — | — | — | — | — | 24 | 24 | 24 | 24 | 24 | — | — |
| `GetISteamController*` | — | — | — | — | — | — | 25 | 25 | 25 | 25 | 25 | 25 | 25 |
| `GetISteamUGC` | — | — | — | — | — | — | 26 | 26 | 26 | 26 | 26 | 26 | 26 |
| `GetISteamAppList*` | — | — | — | — | — | — | 27 | 27 | 27 | 27 | 27 | 27 | 27 |
| `GetISteamMusic*` | — | — | — | — | — | — | 28 | 28 | 28 | 28 | 28 | 28 | 28 |
| `GetISteamMusicRemote*` | — | — | — | — | — | — | — | — | 29 | 29 | 29 | 29 | 29 |
| `GetISteamHTMLSurface*` | — | — | — | — | — | — | — | — | — | 30 | 30 | 30 | 30 |
| `Set_SteamAPI_CPostAPIResultInProcess†` | — | — | — | — | — | — | — | — | — | 31 | 31 | 31 | 31 |
| `Remove_SteamAPI_CPostAPIResultInProcess†` | — | — | — | — | — | — | — | — | — | 32 | 32 | 32 | 32 |
| `Set_SteamAPI_CCheckCallbackRegisteredInProcess†` | — | — | — | — | — | — | — | — | — | 33 | 33 | 33 | 33 |
| `GetISteamInventory*` | — | — | — | — | — | — | — | — | — | — | 34 | 34 | 34 |
| `GetISteamVideo*` | — | — | — | — | — | — | — | — | — | — | 35 | 35 | 35 |
| `GetISteamParentalSettings*` | — | — | — | — | — | — | — | — | — | — | — | 36 | 36 |
| `GetISteamInput*` | — | — | — | — | — | — | — | — | — | — | — | 37 | 37 |
| `GetISteamParties*` | — | — | — | — | — | — | — | — | — | — | — | 38 | 38 |
| `GetISteamRemotePlay*` | — | — | — | — | — | — | — | — | — | — | — | — | 39 |

**Per-version quirks**
- **`GetISteamUGC` first appears at slot 26 in `013` (Goldberg) / `014` (official, 1.29a)**, holds at **26** through `017`, and is still **26** in the official `018`/`019` (1.43/1.46). `007`–`012` have no such slot.
  - **C# bug — `ISteamClient012`:** the C# maps `GetISteamUGC` at slot **26**, but the official `012` (1.17) vtable has only **24** slots (0–23, ending at `GetISteamHTTP`). Slot 26 does not exist in `012`, so this is an **out-of-bounds** mapping (the C# should treat it as a stub, as `007`–`013` do).
  - **C# discrepancy — `ISteamClient018`/`019`:** the C# maps `GetISteamUGC` at slot **27** (the Goldberg layout, which keeps `GetISteamUnifiedMessages` at 25 and `GetISteamController` at 26). The official `018`/`019` **drop** `GetISteamUnifiedMessages`, so `GetISteamController` is 25 and `GetISteamUGC` is **26**.
- **`CreateLocalUser`** is **1-argument** in `007`; **2-argument** (`+ eAccountType`) from `008` onward. `ISteamClient007` binds the 1-arg native form while the managed signature keeps `eAccountType` for interface conformance.
- `RunFrame` is a `STEAM_PRIVATE_API` (`†`) slot present in **every** version (index drifts: **15** in `007`, **18** in `008`, **19** in `009`/`010`/`012`–`017`, **20** in `011`/`018`/`019`). It is the one `†` slot the library *does* map. From `016` the vtable also gains the `Set_SteamAPI_CPostAPIResultInProcess` / `Remove_SteamAPI_CPostAPIResultInProcess` / `Set_SteamAPI_CCheckCallbackRegisteredInProcess` `STEAM_PRIVATE_API` helpers (slots **31–33**; the first two are prefixed `DEPRECATED_` in `018`/`019`) — none of which the library maps.
- **Slot churn between versions** (why the same method's index shifts): `008` drops `GetISteamContentServer`; `009` adds `GetISteamGameServerStats`; `010` adds `BShutdownIfAllPipesClosed`; `011` adds `GetISteamScreenshots` + `GetISteamHTTP`; `012` drops `GetISteamMasterServerUpdater`; `014` adds `GetISteamUnifiedMessages`/`GetISteamController`/`GetISteamUGC`/`GetISteamAppList`/`GetISteamMusic`; `015` adds `GetISteamMusicRemote`; `016` adds `GetISteamHTMLSurface` + the three `STEAM_PRIVATE_API` helpers; `017` adds `GetISteamInventory`/`GetISteamVideo`; `018` adds `GetISteamGameSearch`, **drops** `GetISteamUnifiedMessages`, and adds `GetISteamParentalSettings`/`GetISteamInput`/`GetISteamParties`; `019` adds `GetISteamRemotePlay`.
- **`ISteamClient017` fix:** `GetISteamUser` previously bound through `GetISteamAppsDelegate` (slot **15**) instead of its own `GetISteamUserDelegate` (slot **5**); it now correctly resolves to slot 5.

---

## ISteamApps

Interface-version strings: `STEAMAPPS_INTERFACE_VERSION001` … `009`. The table below lists **every** vtable slot across all nine versions with the 0-based vtable index it occupies (`—` where a version predates that slot). A trailing `*` marks a method that is **not part of the `ISteamApps` interface** this library exposes — every unmarked method is. ISteamApps only ever *appends* members, so a given method keeps the same index from the version it first appears through `009`. The library ships `ISteamApps001`–`009`; `ISteamApps008` is the default. Note that `ISteamApps008` additionally maps the five `009` trailing slots (`GetFileDetails`–`SetDlcContext`, 25–29), and that the `009` vtable itself grew from 33 slots in SDK 1.64 to 35 in 1.65 (adding `SetGamePerformanceSetting`/`SetGameRenderResolution`).

| Method | 001 | 002 | 003 | 004 | 005 | 006 | 007 | 008 | 009 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `GetAppData*` | 0 | — | — | — | — | — | — | — | — |
| `BIsSubscribed` | — | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `BIsLowViolence` | — | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| `BIsCybercafe` | — | 2 | 2 | 2 | 2 | 2 | 2 | 2 | 2 |
| `BIsVACBanned` | — | 3 | 3 | 3 | 3 | 3 | 3 | 3 | 3 |
| `GetCurrentGameLanguage` | — | 4 | 4 | 4 | 4 | 4 | 4 | 4 | 4 |
| `GetAvailableGameLanguages` | — | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 5 |
| `BIsSubscribedApp` | — | 6 | 6 | 6 | 6 | 6 | 6 | 6 | 6 |
| `BIsDlcInstalled` | — | — | 7 | 7 | 7 | 7 | 7 | 7 | 7 |
| `GetEarliestPurchaseUnixTime` | — | — | — | 8 | 8 | 8 | 8 | 8 | 8 |
| `BIsSubscribedFromFreeWeekend` | — | — | — | 9 | 9 | 9 | 9 | 9 | 9 |
| `GetDLCCount` | — | — | — | — | 10 | 10 | 10 | 10 | 10 |
| `BGetDLCDataByIndex` | — | — | — | — | 11 | 11 | 11 | 11 | 11 |
| `InstallDLC` | — | — | — | — | 12 | 12 | 12 | 12 | 12 |
| `UninstallDLC` | — | — | — | — | 13 | 13 | 13 | 13 | 13 |
| `RequestAppProofOfPurchaseKey` | — | — | — | — | 14 | 14 | 14 | 14 | 14 |
| `GetCurrentBetaName` | — | — | — | — | — | 15 | 15 | 15 | 15 |
| `MarkContentCorrupt` | — | — | — | — | — | 16 | 16 | 16 | 16 |
| `GetInstalledDepots` | — | — | — | — | — | 17 | 17 | 17 | 17 |
| `GetAppInstallDir` | — | — | — | — | — | 18 | 18 | 18 | 18 |
| `BIsAppInstalled` | — | — | — | — | — | 19 | 19 | 19 | 19 |
| `GetAppOwner` | — | — | — | — | — | 20 | 20 | 20 | 20 |
| `GetLaunchQueryParam` | — | — | — | — | — | 21 | 21 | 21 | 21 |
| `GetDlcDownloadProgress` | — | — | — | — | — | — | 22 | 22 | 22 |
| `GetAppBuildId` | — | — | — | — | — | — | 23 | 23 | 23 |
| `RequestAllProofOfPurchaseKeys` | — | — | — | — | — | — | — | 24 | 24 |
| `GetFileDetails` | — | — | — | — | — | — | — | — | 25 |
| `GetLaunchCommandLine` | — | — | — | — | — | — | — | — | 26 |
| `BIsSubscribedFromFamilySharing` | — | — | — | — | — | — | — | — | 27 |
| `BIsTimedTrial` | — | — | — | — | — | — | — | — | 28 |
| `SetDlcContext` | — | — | — | — | — | — | — | — | 29 |
| `GetNumBetas` | — | — | — | — | — | — | — | — | 30 |
| `GetBetaInfo` | — | — | — | — | — | — | — | — | 31 |
| `SetActiveBeta` | — | — | — | — | — | — | — | — | 32 |
| `SetGamePerformanceSetting` | — | — | — | — | — | — | — | — | 33 |
| `SetGameRenderResolution` | — | — | — | — | — | — | — | — | 34 |

- **`GetAppData*`** is the *only* slot of the legacy `ISteamApps001` surface (index **0**). It is not part of the modern `ISteamApps` interface (which begins at `BIsSubscribed` from `002` on), hence the `*` — it is exposed in no other version.
- **`ISteamApps002`** … **`007`** bind the subset of the table that exists in each version (the `—` cells are `NotImplementedException` stubs in that class). **`ISteamApps008`** (the default) binds `BIsSubscribed` … `RequestAllProofOfPurchaseKeys` (indices **0–24**, the full official `008` vtable) *plus* the five `009` trailing slots `GetFileDetails` … `SetDlcContext` (indices **25–29**) — those five exist only in the `009` vtable, so `ISteamApps008` relies on the client returning the newer `009`-shaped vtable for them.
- **`ISteamApps009`** binds the full `009` surface, indices **0–34**: `BIsSubscribed` … `SetActiveBeta` (0–32) *plus* `SetGamePerformanceSetting` (33) and `SetGameRenderResolution` (34). The `009` vtable **grew** between SDK **1.64** (33 slots, ending at `SetActiveBeta`) and **1.65** (35 slots); the last two were added in 1.65 while still `STEAMAPPS_INTERFACE_VERSION009`, and the C# `009` bindings match the 1.65 layout.

> Verified against the official `isteamapps.h`: the `008` (1.37) vtable ends at `RequestAllProofOfPurchaseKeys` (index **24**); `GetFileDetails` is `SteamAPICall_t GetFileDetails(const char*)` (size/SHA delivered via the `FileDetailsResult_t` call-result); and the `009` (1.65) vtable continues with `GetLaunchCommandLine`(26), `BIsSubscribedFromFamilySharing`(27), `BIsTimedTrial`(28), `SetDlcContext`(29), `GetNumBetas`(30), `GetBetaInfo`(31), `SetActiveBeta`(32), `SetGamePerformanceSetting`(33), `SetGameRenderResolution`(34).


---

## ISteamUtils

Interface-version strings: `SteamUtils001` … `SteamUtils011`. The library ships only **`ISteamUtils007`** (SDK **1.29a**). The table lists all **25** slots of the Windows `007` vtable with their 0-based index; `*` marks a slot that is **not** mapped in the C# `ISteamUtils007` (which only binds `GetAppID` and `IsAPICallCompleted`).

| Method | 007 |
| --- | --- |
| `GetSecondsSinceAppActive*` | 0 |
| `GetSecondsSinceComputerActive*` | 1 |
| `GetConnectedUniverse*` | 2 |
| `GetServerRealTime*` | 3 |
| `GetIPCountry*` | 4 |
| `GetImageSize*` | 5 |
| `GetImageRGBA*` | 6 |
| `GetCSERIPPort*` | 7 |
| `GetCurrentBatteryPower*` | 8 |
| `GetAppID` | 9 |
| `SetOverlayNotificationPosition*` | 10 |
| `IsAPICallCompleted` | 11 |
| `GetAPICallFailureReason*` | 12 |
| `GetAPICallResult*` | 13 |
| `RunFrame*` | 14 |
| `GetIPCCallCount*` | 15 |
| `SetWarningMessageHook*` | 16 |
| `IsOverlayEnabled*` | 17 |
| `BOverlayNeedsPresent*` | 18 |
| `CheckFileSignature*` | 19 |
| `ShowGamepadTextInput*` | 20 |
| `GetEnteredGamepadTextLength*` | 21 |
| `GetEnteredGamepadTextInput*` | 22 |
| `GetSteamUILanguage*` | 23 |
| `IsSteamRunningInVR*` | 24 |

**Notes**
- The C# `ISteamUtils007` binds exactly two slots — `GetAppID` (**9**) and `IsAPICallCompleted` (**11**) — both in-bounds for the official `007` (1.29a) vtable. ✓ verified.
- `RunFrame` (14) is a plain `virtual` in the 1.29a header (it only becomes `STEAM_PRIVATE_API` in later SDKs); the library does **not** map it here, hence the `*`.
- **PS3-only slots excluded:** the 1.29a header also declares `PostPS3SysutilCallback`, `BIsReadyToShutdown`, `BIsPSNOnline`, and `SetPSNGameBootInviteStrings` between `CheckFileSignature` (19) and `ShowGamepadTextInput`; they are wrapped in `#ifdef _PS3` and therefore do **not** occupy slots in the Windows vtable (so `ShowGamepadTextInput` is 20, not 24).

---

## ISteamUGC

Interface-version strings: `STEAMUGC_INTERFACE_VERSION001` … `016`. (`011` is omitted — the official SDK tags jump from `…VERSION010` to `…VERSION012`). `004`/`005`/`006` exist only in the Goldberg Emulator SDK, the sole source for those; every other version comes from the official `steamworks_sdk`. The table below lists **every method in the `005`–`016` vtables**, in `016` layout order, with the 0-based vtable index each occupies per version (`—` where a version predates that slot; a cell holding two numbers is an overloaded method occupying two consecutive slots). Unlike ISteamApps the UGC vtable grows by *inserting* members mid-interface, so a method's index can change between versions (e.g. `DownloadItem` drifts 45 → 72). Every method listed is part of the library's `ISteamUGC` interface — there are no `*` (unmapped) rows for UGC.

| Method | 005 | 006 | 007 | 008 | 009 | 010 | 012 | 013 | 014 | 015 | 016 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `CreateQueryUserUGCRequest` | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `CreateQueryAllUGCRequest` | 1 | 1 | 1 | 1 | 1 | 1 | 1, 2 | 1, 2 | 1, 2 | 1, 2 | 1, 2 |
| `CreateQueryUGCDetailsRequest` | 2 | 2 | 2 | 2 | 2 | 2 | 3 | 3 | 3 | 3 | 3 |
| `SendQueryUGCRequest` | 3 | 3 | 3 | 3 | 3 | 3 | 4 | 4 | 4 | 4 | 4 |
| `GetQueryUGCResult` | 4 | 4 | 4 | 4 | 4 | 4 | 5 | 5 | 5 | 5 | 5 |
| `GetQueryUGCNumTags` | — | — | — | — | — | — | — | — | — | 6 | 6 |
| `GetQueryUGCTag` | — | — | — | — | — | — | — | — | — | 7 | 7 |
| `GetQueryUGCTagDisplayName` | — | — | — | — | — | — | — | — | — | 8 | 8 |
| `GetQueryUGCPreviewURL` | 5 | 5 | 5 | 5 | 5 | 5 | 6 | 6 | 6 | 9 | 9 |
| `GetQueryUGCMetadata` | 6 | 6 | 6 | 6 | 6 | 6 | 7 | 7 | 7 | 10 | 10 |
| `GetQueryUGCChildren` | 7 | 7 | 7 | 7 | 7 | 7 | 8 | 8 | 8 | 11 | 11 |
| `GetQueryUGCStatistic` | 8 | 8 | 8 | 8 | 8 | 8 | 9 | 9 | 9 | 12 | 12 |
| `GetQueryUGCNumAdditionalPreviews` | 9 | 9 | 9 | 9 | 9 | 9 | 10 | 10 | 10 | 13 | 13 |
| `GetQueryUGCAdditionalPreview` | 10 | 10 | 10 | 10 | 10 | 10 | 11 | 11 | 11 | 14 | 14 |
| `GetQueryUGCNumKeyValueTags` | — | — | 11 | 11 | 11 | 11 | 12 | 12 | 12 | 15 | 15 |
| `GetQueryUGCKeyValueTag` | — | — | 12 | 12 | 12 | 12 | 13 | 13, 14 | 13, 14 | 16, 17 | 16, 17 |
| `ReleaseQueryUGCRequest` | 11 | 11 | 13 | 13 | 13 | 13 | 14 | 15 | 15 | 18 | 18 |
| `AddRequiredTag` | 12 | 12 | 14 | 14 | 14 | 14 | 15 | 16 | 16 | 19 | 19 |
| `AddRequiredTagGroup` | — | — | — | — | — | — | — | — | 17 | 20 | 20 |
| `AddExcludedTag` | 13 | 13 | 15 | 15 | 15 | 15 | 16 | 17 | 18 | 21 | 21 |
| `SetReturnOnlyIDs` | — | — | — | — | 16 | 16 | 17 | 18 | 19 | 22 | 22 |
| `SetReturnKeyValueTags` | — | — | 16 | 16 | 17 | 17 | 18 | 19 | 20 | 23 | 23 |
| `SetReturnLongDescription` | 14 | 14 | 17 | 17 | 18 | 18 | 19 | 20 | 21 | 24 | 24 |
| `SetReturnMetadata` | 15 | 15 | 18 | 18 | 19 | 19 | 20 | 21 | 22 | 25 | 25 |
| `SetReturnChildren` | 16 | 16 | 19 | 19 | 20 | 20 | 21 | 22 | 23 | 26 | 26 |
| `SetReturnAdditionalPreviews` | 17 | 17 | 20 | 20 | 21 | 21 | 22 | 23 | 24 | 27 | 27 |
| `SetReturnTotalOnly` | 18 | 18 | 21 | 21 | 22 | 22 | 23 | 24 | 25 | 28 | 28 |
| `SetReturnPlaytimeStats` | — | — | — | — | — | 23 | 24 | 25 | 26 | 29 | 29 |
| `SetLanguage` | — | 19 | 22 | 22 | 23 | 24 | 25 | 26 | 27 | 30 | 30 |
| `SetAllowCachedResponse` | 19 | 20 | 23 | 23 | 24 | 25 | 26 | 27 | 28 | 31 | 31 |
| `SetCloudFileNameFilter` | 20 | 21 | 24 | 24 | 25 | 26 | 27 | 28 | 29 | 32 | 32 |
| `SetMatchAnyTag` | 21 | 22 | 25 | 25 | 26 | 27 | 28 | 29 | 30 | 33 | 33 |
| `SetSearchText` | 22 | 23 | 26 | 26 | 27 | 28 | 29 | 30 | 31 | 34 | 34 |
| `SetRankedByTrendDays` | 23 | 24 | 27 | 27 | 28 | 29 | 30 | 31 | 32 | 35 | 35 |
| `SetTimeCreatedDateRange` | — | — | — | — | — | — | — | — | — | — | 36 |
| `SetTimeUpdatedDateRange` | — | — | — | — | — | — | — | — | — | — | 37 |
| `AddRequiredKeyValueTag` | — | — | 28 | 28 | 29 | 30 | 31 | 32 | 33 | 36 | 38 |
| `RequestUGCDetails` | 24 | 25 | 29 | 29 | 30 | 31 | 32 | 33 | 34 | 37 | 39 |
| `CreateItem` | 25 | 26 | 30 | 30 | 31 | 32 | 33 | 34 | 35 | 38 | 40 |
| `StartItemUpdate` | 26 | 27 | 31 | 31 | 32 | 33 | 34 | 35 | 36 | 39 | 41 |
| `SetItemTitle` | 27 | 28 | 32 | 32 | 33 | 34 | 35 | 36 | 37 | 40 | 42 |
| `SetItemDescription` | 28 | 29 | 33 | 33 | 34 | 35 | 36 | 37 | 38 | 41 | 43 |
| `SetItemUpdateLanguage` | — | 30 | 34 | 34 | 35 | 36 | 37 | 38 | 39 | 42 | 44 |
| `SetItemMetadata` | 29 | 31 | 35 | 35 | 36 | 37 | 38 | 39 | 40 | 43 | 45 |
| `SetItemVisibility` | 30 | 32 | 36 | 36 | 37 | 38 | 39 | 40 | 41 | 44 | 46 |
| `SetItemTags` | 31 | 33 | 37 | 37 | 38 | 39 | 40 | 41 | 42 | 45 | 47 |
| `SetItemContent` | 32 | 34 | 38 | 38 | 39 | 40 | 41 | 42 | 43 | 46 | 48 |
| `SetItemPreview` | 33 | 35 | 39 | 39 | 40 | 41 | 42 | 43 | 44 | 47 | 49 |
| `SetAllowLegacyUpload` | — | — | — | — | — | — | 43 | 44 | 45 | 48 | 50 |
| `RemoveAllItemKeyValueTags` | — | — | — | — | — | — | — | 45 | 46 | 49 | 51 |
| `RemoveItemKeyValueTags` | — | — | 40 | 40 | 41 | 42 | 44 | 46 | 47 | 50 | 52 |
| `AddItemKeyValueTag` | — | — | 41 | 41 | 42 | 43 | 45 | 47 | 48 | 51 | 53 |
| `AddItemPreviewFile` | — | — | — | 42 | 43 | 44 | 46 | 48 | 49 | 52 | 54 |
| `AddItemPreviewVideo` | — | — | — | 43 | 44 | 45 | 47 | 49 | 50 | 53 | 55 |
| `UpdateItemPreviewFile` | — | — | — | 44 | 45 | 46 | 48 | 50 | 51 | 54 | 56 |
| `UpdateItemPreviewVideo` | — | — | — | 45 | 46 | 47 | 49 | 51 | 52 | 55 | 57 |
| `RemoveItemPreview` | — | — | — | 46 | 47 | 48 | 50 | 52 | 53 | 56 | 58 |
| `SubmitItemUpdate` | 34 | 36 | 42 | 47 | 48 | 49 | 51 | 53 | 54 | 57 | 59 |
| `GetItemUpdateProgress` | 35 | 37 | 43 | 48 | 49 | 50 | 52 | 54 | 55 | 58 | 60 |
| `SetUserItemVote` | — | 38 | 44 | 49 | 50 | 51 | 53 | 55 | 56 | 59 | 61 |
| `GetUserItemVote` | — | 39 | 45 | 50 | 51 | 52 | 54 | 56 | 57 | 60 | 62 |
| `AddItemToFavorites` | 36 | 40 | 46 | 51 | 52 | 53 | 55 | 57 | 58 | 61 | 63 |
| `RemoveItemFromFavorites` | 37 | 41 | 47 | 52 | 53 | 54 | 56 | 58 | 59 | 62 | 64 |
| `SubscribeItem` | 38 | 42 | 48 | 53 | 54 | 55 | 57 | 59 | 60 | 63 | 65 |
| `UnsubscribeItem` | 39 | 43 | 49 | 54 | 55 | 56 | 58 | 60 | 61 | 64 | 66 |
| `GetNumSubscribedItems` | 40 | 44 | 50 | 55 | 56 | 57 | 59 | 61 | 62 | 65 | 67 |
| `GetSubscribedItems` | 41 | 45 | 51 | 56 | 57 | 58 | 60 | 62 | 63 | 66 | 68 |
| `GetItemState` | 42 | 46 | 52 | 57 | 58 | 59 | 61 | 63 | 64 | 67 | 69 |
| `GetItemInstallInfo` | 43 | 47 | 53 | 58 | 59 | 60 | 62 | 64 | 65 | 68 | 70 |
| `GetItemDownloadInfo` | 44 | 48 | 54 | 59 | 60 | 61 | 63 | 65 | 66 | 69 | 71 |
| `DownloadItem` | 45 | 49 | 55 | 60 | 61 | 62 | 64 | 66 | 67 | 70 | 72 |
| `BInitWorkshopForGameServer` | — | — | — | 61 | 62 | 63 | 65 | 67 | 68 | 71 | 73 |
| `SuspendDownloads` | — | — | — | 62 | 63 | 64 | 66 | 68 | 69 | 72 | 74 |
| `StartPlaytimeTracking` | — | — | — | — | 64 | 65 | 67 | 69 | 70 | 73 | 75 |
| `StopPlaytimeTracking` | — | — | — | — | 65 | 66 | 68 | 70 | 71 | 74 | 76 |
| `StopPlaytimeTrackingForAllItems` | — | — | — | — | 66 | 67 | 69 | 71 | 72 | 75 | 77 |
| `AddDependency` | — | — | — | — | — | 68 | 70 | 72 | 73 | 76 | 78 |
| `RemoveDependency` | — | — | — | — | — | 69 | 71 | 73 | 74 | 77 | 79 |
| `AddAppDependency` | — | — | — | — | — | — | 72 | 74 | 75 | 78 | 80 |
| `RemoveAppDependency` | — | — | — | — | — | — | 73 | 75 | 76 | 79 | 81 |
| `GetAppDependencies` | — | — | — | — | — | — | 74 | 76 | 77 | 80 | 82 |
| `DeleteItem` | — | — | — | — | — | — | 75 | 77 | 78 | 81 | 83 |
| `ShowWorkshopEULA` | — | — | — | — | — | — | — | — | — | — | 84 |
| `GetWorkshopEULAStatus` | — | — | — | — | — | — | — | — | — | — | 85 |

**Notes**
- **`*` convention** — as in the ISteamApps table above, a trailing `*` marks a method that is *not* part of the library's `ISteamUGC` interface. Every method in the table here *is* part of that interface, so there are no `*` rows.
- The four overloads no UGC vtable exposes — the 1-arg `RequestUGCDetails`, `GetItemUpdateInfo`, and the two non-timestamp `GetItemInstallInfo` overloads — stay `NotImplementedException` stubs in every version (they have no vtable slot, so they are absent from the table).
- `DownloadItem` is `bool DownloadItem(PublishedFileId_t, bool)` in every version that has it (`004` onward); its index drifts 45 → 49 → 55 → 60 → 61 → 62 → 64 → 66 → 67 → 70 → 72 across the versions shown.
- The newer versions (`006`–`016`) also expose the functions that this pass adds to the `ISteamUGC` contract (key-value tags, tag groups, the `SetReturn*`/`SetLanguage`/`SetAllowCachedResponse`/`SetTime*DateRange` query options, item key-value tags and previews, user votes, playtime tracking, dependencies, `DeleteItem`, `BInitWorkshopForGameServer`, `SuspendDownloads`, and the Workshop EULA). Each is now a real P/Invoke mapping where the version's vtable has the slot and an `[Obsolete]` stub where it does not (this is also why the indices of the *shared* methods keep growing across versions).
- **Scope** — the table covers the `005`–`016` snapshots (`001`–`004` are legacy; the library ships `ISteamUGC005`–`016`). The official SDK has since advanced to `STEAMUGC_INTERFACE_VERSION021` (≈99 vtable slots); the `017`–`021` additions are **not** part of the library's `ISteamUGC` contract — unlike the `ISteamApps` `009` methods, which the library carries as forward-compat stubs — so they are intentionally out of scope here.

