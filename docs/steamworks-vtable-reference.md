# Steamworks VTable Reference

Authoritative per-version layouts for the three interface families this library exposes — `ISteamApps`, `ISteamClient`, and `ISteamUGC`. Every `[VTableIndex]` in the generated `ISteam*NNN.cs` classes is checked against the official Steamworks SDK (`TempRefs/steamworks_sdk`, git-tagged per SDK release — the tag carrying each interface version supplies that version's `isteam*.h`). The only snapshots the official repo does not tag — `ISteamClient013`/`014` and `ISteamUGC004`/`005`/`006` — are taken from the Goldberg Emulator SDK (`TempRefs/goldberg_emulator/sdk_includes/isteam*NNN.h`), the sole source for those.

**Counting rules** (the source of most index-mapping errors):
- **`#ifdef _PS3`** members are *excluded* from the count — they are not part of the Windows vtable.
- **`STEAM_PRIVATE_API(x)`** expands to `protected: x public:`, so those methods *do* occupy a vtable slot and *are* counted.
- `DEPRECATED_*` helpers likewise occupy slots and are counted.
- Each interface's **first vtable slot is its version-string getter** (e.g. `ISteamApps_VersionNum()` → `"STEAMAPPS_INTERFACE_VERSION008"`); the generated `[VTableIndex]` values are the 0-based function indices used in the classes (matching the interface method order).

> Because Steamworks inserts new functions **into the middle** of these vtables between versions, the same method resolves to a different index in each version — each generated class therefore carries its own `[VTableIndex]` set. Use the class whose interface-version string matches the one you requested.

---

## ISteamClient (entry-point client)

Interface-version strings: `SteamClient007` … `SteamClient019`. The contract exposes four `GetISteam*` factory slots plus `CreateSteamPipe` / `BReleaseSteamPipe` / `ConnectToGlobalUser` / `CreateLocalUser` / `ReleaseUser` and the lifecycle helpers `SetLocalIPBinding` / `RunFrame` / `SetWarningMessageHook` (backed by the `SteamAPIWarningMessageHook_t` delegate) / `BShutdownIfAllPipesClosed`. Steam interleaves *other* `GetISteam*` accessors (Friends, Matchmaking, RemoteStorage, HTTP, Controller, Input, …) between the slots this library exposes, so each exposed slot's index varies by version:

| Version | `GetISteamUser` | `GetISteamUtils` | `GetISteamApps` | `GetISteamUGC` |
| --- | --- | --- | --- | --- |
| `007` | 5 | 9 | 18 | — (stub) |
| `008` | 5 | 9 | 15 | — (stub) |
| `009`–`011` | 5 | 9 | 16 | — (stub) |
| `012`–`016` | 5 | 9 | 15 | 26 |
| `017` | 5 | 9 | 15 | 26 |
| `018`–`019` | 5 | 9 | 15 | 27 |

**Per-version quirks**
- **`GetISteamUGC` first appears in `ISteamClient012`** (vtable **26**). `007`–`011` therefore declare it as a `NotImplementedException` stub. In `018`/`019` it moves to **27** (a `DEPRECATED_GetISteamUGC` slot precedes it).
- **`CreateLocalUser`** is **1-argument** in `007`; **2-argument** (`+ eAccountType`) from `008` onward. `ISteamClient007` binds the 1-arg native form while the managed signature keeps `eAccountType` for interface conformance.
- `RunFrame` is a `STEAM_PRIVATE_API` slot present in **every** version (its index drifts — e.g. **15** in `007`, **18** in `008`, **19** in `009`/`012`–`017`, **20** in `011`/`018`/`019`); `ISteamClient018`/`019` additionally add `DEPRECATED_*` helpers. All of these occupy and count vtable slots.
- **`ISteamClient017` fix:** `GetISteamUser` previously bound through `GetISteamAppsDelegate` (slot **15**) instead of its own `GetISteamUserDelegate` (slot **5**); it now correctly resolves to slot 5.

---

## ISteamApps

Interface-version strings: `STEAMAPPS_INTERFACE_VERSION001` … `008`.  Representative slots (the full per-version layout is in the class):

| Index (008) | Method |
| --- | --- |
| 11 | `BGetDLCDataByIndex` |
| 14 | `RequestAppProofOfPurchaseKey` |
| 15 | `GetCurrentBetaName` |
| 16 | `MarkContentCorrupt` |
| 17 | `GetInstalledDepots` |
| 20 | `GetAppOwner` |
| 21 | `GetLaunchQueryParam` |
| 22 | `GetDlcDownloadProgress` |
| 23 | `GetAppBuildId` |
| 24 | `RequestAllProofOfPurchaseKeys` |
| 25 | `GetFileDetails(string)` |
| 26 | `GetLaunchCommandLine()` |
| 27 | `BIsSubscribedFromFamilySharing()` |
| 28 | `BIsTimedTrial(ref, ref)` |
| 29 | `SetDlcContext(UInt32)` |

- **`ISteamApps002`** exposes the first seven functions (`BIsSubscribed`…`BIsSubscribedApp`, vtable 0–6); the remaining 23 are `NotImplementedException` stubs.
- **`ISteamApps001`** is a legacy contract whose only native function (`GetAppData`) is not part of the current `ISteamApps` surface, so all 30 mapped functions are stubs there.
- **`ISteamApps008`** (the default) maps all 30 as real P/Invoke at the indices above.

> Verified: `GetFileDetails` is `SteamAPICall_t GetFileDetails(const char*)` (size/SHA delivered via the `FileDetailsResult_t` call-result), and `BIsSubscribedFromFamilySharing` sits at index **27** (26 is `GetLaunchCommandLine`) — both confirmed against the official `isteamapps.h` (`STEAMAPPS_INTERFACE_VERSION008` for the core, `STEAMAPPS_INTERFACE_VERSION009` for these trailing methods).


---

## ISteamUGC

Interface-version strings: `STEAMUGC_INTERFACE_VERSION001` … `016`. (`011` is omitted — the official SDK tags jump from `…VERSION010` to `…VERSION012`). `004`/`005`/`006` exist only in the Goldberg Emulator SDK, the sole source for those; every other version comes from the official `steamworks_sdk`. The table shows the **46 base-interface methods** mapped by the library, in interface order, with their vtable index in each generated version (0-based, as used in the generated class):

| Method | 005 | 006 | 007 | 008 | 009 | 010 | 012 | 013 | 014 | 015 | 016 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `CreateQueryUserUGCRequest` | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `CreateQueryAllUGCRequest` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| `CreateQueryUGCDetailsRequest` | 2 | 2 | 2 | 2 | 2 | 2 | 3 | 3 | 3 | 3 | 3 |
| `SendQueryUGCRequest` | 3 | 3 | 3 | 3 | 3 | 3 | 4 | 4 | 4 | 4 | 4 |
| `GetQueryUGCResult` | 4 | 4 | 4 | 4 | 4 | 4 | 5 | 5 | 5 | 5 | 5 |
| `GetQueryUGCPreviewURL` | 5 | 5 | 5 | 5 | 5 | 5 | 6 | 6 | 6 | 9 | 9 |
| `GetQueryUGCMetadata` | 6 | 6 | 6 | 6 | 6 | 6 | 7 | 7 | 7 | 10 | 10 |
| `GetQueryUGCChildren` | 7 | 7 | 7 | 7 | 7 | 7 | 8 | 8 | 8 | 11 | 11 |
| `GetQueryUGCStatistic` | 8 | 8 | 8 | 8 | 8 | 8 | 9 | 9 | 9 | 12 | 12 |
| `GetQueryUGCNumAdditionalPreviews` | 9 | 9 | 9 | 9 | 9 | 9 | 10 | 10 | 10 | 13 | 13 |
| `GetQueryUGCAdditionalPreview` | 10 | 10 | 10 | 10 | 10 | 10 | 11 | 11 | 11 | 14 | 14 |
| `ReleaseQueryUGCRequest` | 11 | 11 | 13 | 13 | 13 | 13 | 14 | 15 | 15 | 18 | 18 |
| `AddRequiredTag` | 12 | 12 | 14 | 14 | 14 | 14 | 15 | 16 | 16 | 19 | 19 |
| `AddExcludedTag` | 13 | 13 | 15 | 15 | 15 | 15 | 16 | 17 | 18 | 21 | 21 |
| `SetReturnLongDescription` | 14 | 14 | 17 | 17 | 18 | 18 | 19 | 20 | 21 | 24 | 24 |
| `SetReturnMetadata` | 15 | 15 | 18 | 18 | 19 | 19 | 20 | 21 | 22 | 25 | 25 |
| `SetReturnChildren` | 16 | 16 | 19 | 19 | 20 | 20 | 21 | 22 | 23 | 26 | 26 |
| `SetReturnAdditionalPreviews` | 17 | 17 | 20 | 20 | 21 | 21 | 22 | 23 | 24 | 27 | 27 |
| `SetReturnTotalOnly` | 18 | 18 | 21 | 21 | 22 | 22 | 23 | 24 | 25 | 28 | 28 |
| `SetAllowCachedResponse` | 19 | 20 | 23 | 23 | 24 | 25 | 26 | 27 | 28 | 31 | 31 |
| `SetCloudFileNameFilter` | 20 | 21 | 24 | 24 | 25 | 26 | 27 | 28 | 29 | 32 | 32 |
| `SetMatchAnyTag` | 21 | 22 | 25 | 25 | 26 | 27 | 28 | 29 | 30 | 33 | 33 |
| `SetSearchText` | 22 | 23 | 26 | 26 | 27 | 28 | 29 | 30 | 31 | 34 | 34 |
| `SetRankedByTrendDays` | 23 | 24 | 27 | 27 | 28 | 29 | 30 | 31 | 32 | 35 | 35 |
| `RequestUGCDetails` (2-arg) | 24 | 25 | 29 | 29 | 30 | 31 | 32 | 33 | 34 | 37 | 39 |
| `CreateItem` | 25 | 26 | 30 | 30 | 31 | 32 | 33 | 34 | 35 | 38 | 40 |
| `StartItemUpdate` | 26 | 27 | 31 | 31 | 32 | 33 | 34 | 35 | 36 | 39 | 41 |
| `SetItemTitle` | 27 | 28 | 32 | 32 | 33 | 34 | 35 | 36 | 37 | 40 | 42 |
| `SetItemDescription` | 28 | 29 | 33 | 33 | 34 | 35 | 36 | 37 | 38 | 41 | 43 |
| `SetItemMetadata` | 29 | 31 | 35 | 35 | 36 | 37 | 38 | 39 | 40 | 43 | 45 |
| `SetItemVisibility` | 30 | 32 | 36 | 36 | 37 | 38 | 39 | 40 | 41 | 44 | 46 |
| `SetItemTags` | 31 | 33 | 37 | 37 | 38 | 39 | 40 | 41 | 42 | 45 | 47 |
| `SetItemContent` | 32 | 34 | 38 | 38 | 39 | 40 | 41 | 42 | 43 | 46 | 48 |
| `SetItemPreview` | 33 | 35 | 39 | 39 | 40 | 41 | 42 | 43 | 44 | 47 | 49 |
| `SubmitItemUpdate` | 34 | 36 | 42 | 47 | 48 | 49 | 51 | 53 | 54 | 57 | 59 |
| `GetItemUpdateProgress` | 35 | 37 | 43 | 48 | 49 | 50 | 52 | 54 | 55 | 58 | 60 |
| `AddItemToFavorites` | 36 | 40 | 46 | 51 | 52 | 53 | 55 | 57 | 58 | 61 | 63 |
| `RemoveItemFromFavorites` | 37 | 41 | 47 | 52 | 53 | 54 | 56 | 58 | 59 | 62 | 64 |
| `SubscribeItem` | 38 | 42 | 48 | 53 | 54 | 55 | 57 | 59 | 60 | 63 | 65 |
| `UnsubscribeItem` | 39 | 43 | 49 | 54 | 55 | 56 | 58 | 60 | 61 | 64 | 66 |
| `GetNumSubscribedItems` | 40 | 44 | 50 | 55 | 56 | 57 | 59 | 61 | 62 | 65 | 67 |
| `GetSubscribedItems` | 41 | 45 | 51 | 56 | 57 | 58 | 60 | 62 | 63 | 66 | 68 |
| `GetItemState` | 42 | 46 | 52 | 57 | 58 | 59 | 61 | 63 | 64 | 67 | 69 |
| `GetItemInstallInfo` (timestamp) | 43 | 47 | 53 | 58 | 59 | 60 | 62 | 64 | 65 | 68 | 70 |
| `GetItemDownloadInfo` | 44 | 48 | 54 | 59 | 60 | 61 | 63 | 65 | 66 | 69 | 71 |
| `DownloadItem` | 45 | 49 | 55 | 60 | 61 | 62 | 64 | 66 | 67 | 70 | 72 |

**Notes**
- The four methods no UGC vtable exposes — the 1-arg `RequestUGCDetails`, `GetItemUpdateInfo`, and the two non-timestamp `GetItemInstallInfo` overloads — stay `NotImplementedException` stubs in every version.
- `DownloadItem` is `bool DownloadItem(PublishedFileId_t, bool)` in every version that has it (`004` onward); its index drifts 45 → 49 → 55 → 60 → 61 → 62 → 64 → 66 → 67 → 70 → 72 across the versions shown.
- The newer versions (`006`–`016`) also expose the functions that this pass adds to the `ISteamUGC` contract (key-value tags, tag groups, the `SetReturn*`/`SetLanguage`/`SetAllowCachedResponse`/`SetTime*DateRange` query options, item key-value tags and previews, user votes, playtime tracking, dependencies, `DeleteItem`, `BInitWorkshopForGameServer`, `SuspendDownloads`, and the Workshop EULA). Each is now a real P/Invoke mapping where the version's vtable has the slot and an `[Obsolete]` stub where it does not (this is also why the indices of the *shared* methods keep growing across versions).

