# SteamVent
A .NET library for interfacing with the Steam client application via IPC (Steamworks interproc), the registry, and file-system access to the local Steam installation.

SteamVent is the buisness logic for the following projects:
* **BZR Mod Manager**

It is organized into three assemblies:

| Project | Target | Purpose |
| --- | --- | --- |
| `SteamVent.Core` | `net10.0` | Framework-agnostic abstractions: `PublishedFileData`, `WorkshopItemStatus`, the workshop `IWorkshopSource` / `IWorkshopCommands` contracts, the unified `WorkshopException`, `SteamVentOptions` (per-subsystem enable flags + `SteamworksAppId`), the `BVdf` binary-VDF reader, and the Steam result structs (`DownloadItemResult_t`, `RemoteStorage*Result_t`, `EItemState`, `EResult`, `SteamTypes`). |
| `SteamVent.Bridge` | `net10.0` | The process-isolated IPC **Bridge** (`BridgeContext` + the `ISteamApps`/`ISteamUtils`/`ISteamUGC` command wrappers and their generated proxies). |
| `SteamVent` | `net10.0` | The main library: the `Steamworks` (native interproc) layer, the `FileSystem` layer, and the `SteamContext` facade, which deliberately composes whichever subsystems are enabled per operation. |

> The two C++ projects (`steamcmdinj`, `steamcmdprox`) implement the `steamcmd` injection/proxy used by the Bridge child process and build with MSBuild, not the `dotnet` CLI.

---

## Disclaimer
The SteamVent.Steamworks.Native interfaces were developed solely to achieve interoperability with existing platforms, created in strict accordance with the reverse engineering and interoperability exemptions provided under Section 1201(f) of the Digital Millennium Copyright Act (DMCA).

The Official Steamworks API and Goldberg Emulator were used as sources of truth specifically and solely for the versioned vtable layouts of the Steamworks ABI.  No business logic from Steamworks or the Goldberg Emulator was referenced or copied in the creation of these interfaces or the surrounding logic.

AI assistance was used for updating and extending the existing created-by-hand SteamVent native interfaces. These AI tools were used to update the ABI interface meta-programming to utilize UTF8 properly in string byte streams as well as to extend and fill in missing vtable functions and unimplemented interface versions.

---

## Project structure

```
SteamVent/
├── SteamVent.Core/              # net10.0
│   ├── Source.cs                # Source, PublishedFileData, IWorkshopSource, IWorkshopCommands
│   ├── WorkshopItemProgress.cs  # live download-progress sample (state + bytes + Percent)
│   ├── BVdf.cs                  # Binary VDF reader (BVdf, BVProperty, BVPropertyCollection, ...)
│   └── InterProc/               # Steam IPC result enums/structs
│       ├── SteamTypes.cs        # EResult, CallResult<T>, ISteamAPICall
│       ├── EItemState.cs        # EItemState / EItemPreview flags
│       ├── DownloadItemResult_t.cs
│       ├── RemoteStorageSubscribePublishedFileResult_t.cs
│       └── RemoteStorageUnsubscribePublishedFileResult_t.cs
├── SteamVent.Bridge/            # net10.0, depends on Core
│   ├── BridgeContext.cs         # IPC client: launches a Bridge child process, sends commands
│   ├── BridgePool.cs            # warm per-AppId BridgeContext pool with idle reaping
│   ├── BridgeSource.cs          # IWorkshopSource / IWorkshopCommands over the Bridge (via the pool)
│   ├── Interfaces/              # ISteamApps / ISteamUtils / ISteamUGC command contracts
│   └── InterProc/               # SteamAPICall_t, Callback<T>, generated proxies + result structs
└── SteamVent/                   # net10.0, depends on Core
    ├── SteamContext.cs          # Public gateway / orchestrator: owns Options +
    │                            # the Steamworks lifecycle + optional-module resolution;
    │                            # delegates to the facades
    ├── SteamApps.cs             # SteamVent app facade (ctx.SteamApps) — OWNS app queries
    │                            # (owned apps, metadata, install dir, engine mods);
    │                            # composes in-process Steamworks with the filesystem fallback
    ├── SteamWorkshop.cs         # SteamVent workshop facade (ctx.SteamWorkshop) — OWNS workshop reads +
    │                            # appid-context commands; composes in-process, Bridge, SteamCmd, or disk
    ├── SteamLaunchable.cs       # SteamLaunchable* POCOs returned by the context
    ├── Steamworks/              # In-process Steamworks layer (namespace SteamVent.Steamworks)
    │   ├── SteamClient.cs       #   Owned lifecycle: pipe/user + appid (request→latch) + interop factory + wrappers
    │   ├── SteamApps.cs         #   Thin, non-throwing wrapper over ISteamApps (ownership / install dir)
    │   ├── SteamWorkshop.cs     #   Thin wrapper over ISteamUGC (in-process IWorkshopCommands/IWorkshopSource)
    │   └── Native/              # namespace SteamVent.Steamworks.Native
    │       ├── Steam.cs         #   Steam interop helper (CreateInterface / GetAPICallResult / ...)
    │       ├── Attributes/      #   [InterfaceVersion], [VTableIndex]
    │       ├── Interfaces/      #   ISteamApps(+versioned), ISteamClient, ISteamUGC, ISteamUtils, CSteamID
    │       ├── Interop/         #   SteamInterfaceWrapper
    │       └── Native/          #   SteamNative, SysNative
    └── FileSystem/              # File-system access to a local Steam install (namespace SteamVent.FileSystem)
        ├── SteamAppInfoDataFile.cs  # appcache (appinfo.vdf) parser + GetApps()
        ├── SteamAppInfo.cs          # strongly-typed per-app model (SteamAppInfo / Common / Extended)
        ├── BvdfNode.cs              # lossless VDF node base + generic object node (Extra + [string] indexer)
        ├── SteamProcessInfo.cs      # Steam install path, library paths, running-process detection
        └── SteamWorkshop.cs         # Read workshop items straight from the workshop/contentdl cache
```

---

## Core abstractions (`SteamVent.Core`)

- **`PublishedFileData`** — a lightweight snapshot of one Steam Workshop item: `PublishedFileId`, `State` (`EItemState` flags), plus optional `Folder`, `SizeOnDisk`, `TimeStamp`, `BytesDownloaded`, `BytesTotal`.
- **`WorkshopItemProgress`** — a point-in-time sample of an in-flight download: `PublishedFileId`, live `State`, `BytesDownloaded` / `BytesTotal`, and a `Percent` convenience. Raised through `IProgress<WorkshopItemProgress>` by both backends so a caller can surface live transfer status.
- **`IWorkshopSource`** — read-side workshop listing:
  ```csharp
  Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken ct = default);
  Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken ct = default);
  ```
- **`IWorkshopCommands`** — command-side workshop operations:
  ```csharp
  Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeAsync(ulong publishedFileId, int appid, CancellationToken ct = default);
  Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken ct = default);
  // Completes only when the item is fully installed (not on the ACK); reports live progress when given.
  Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, IProgress<WorkshopItemProgress>? progress = null, CancellationToken ct = default);
  ```
- **`WorkshopException`** — the unified exception for workshop operations (subscribe / download / unsubscribe and the AppID-context checks around them). Callers catch this one type no matter which backend answered: the in-process `Steamworks` layer throws it directly, and the `Bridge` boxes its richer `BridgeException` in it as `InnerException`. It carries `AppId`, `PublishedFileId`, `Operation`, and an `IsAppIdMismatch` flag (set when the requested AppID is not the one Steam is actually running under — the "stale `steam_appid.txt`" / wrong-context condition); `WorkshopException.AppIdMismatch(...)` builds that case.
- **`BVdf`** — a reader for Valve's binary `.vdf` format (the appcache `appinfo.vdf`), exposing `BVPropertyCollection` / `BVProperty` with typed `GetValue<T>()` accessors.

---

## The Steamworks layer (`SteamVent.Steamworks`)

This is the in-process, native-interproc layer (renamed from the former `SteamVent.InterProc` namespace to `SteamVent.Steamworks` / `SteamVent.Steamworks.Native`).

> **String marshaling is UTF-8.** Steam's interop wire format is UTF-8, so any string that crosses the native boundary is marshaled explicitly as UTF-8: outgoing arguments are converted with `Marshal.StringToCoTaskMemUTF8` (released with `Marshal.ZeroFreeCoTaskMemUTF8` after the call) and returned strings are read back with `Marshal.PtrToStringUTF8`. This keeps non-ASCII content (accents, CJK, emoji, …) intact instead of being mangled through the Windows ANSI code page.

> **Prefer the `SteamContext` facades.** In normal use you do not touch this layer directly — `SteamContext` owns the
> `SteamClient` for you (it is no longer exposed publicly) and hands you the **`SteamVent.SteamApps`** (`ctx.SteamApps`)
> and **`SteamVent.SteamWorkshop`** (`ctx.SteamWorkshop`) facades, which route every call in accordance with your
> configured sources (see the `SteamContext` section below). The raw types documented here remain available if you want
> to drive a child implementation directly.

### `SteamClient` — owns the Steamworks lifecycle

A ref-counted singleton (`Construct()` / `Dispose()`). It never requires Steam to be running; the interop is started lazily and is safe to query at any time.

```csharp
using SteamVent.Steamworks;

SteamClient client = SteamClient.Construct();
try
{
    if (!client.EnsureSteamworks())   // idempotent; false when Steam is not running
    {
        // handle "Steam is not running"
    }

    ISteamApps?   apps   = client.GetISteamApps();   // null when unavailable
    ISteamUtils?  utils  = client.GetISteamUtils();
    UInt32 appId   = utils.GetAppID();     // the live appid this process runs under (source of truth)
    ISteamUGC?    ugc    = client.GetISteamUGC();
}
finally
{
    client.Dispose();   // releases pipe/user when the last reference goes away
}
```

### `SteamApps` — thin, non-throwing wrapper over `ISteamApps`

```csharp
SteamApps apps = client.GetSteamApps();

bool? installed = apps.GetAppInstalled(appId);     // null when Steamworks is unavailable
bool? owned     = apps.IsSubscribed(appId);
string? dir     = apps.GetAppInstallDir(appId);
```

### `SteamWorkshop` — thin wrapper over `ISteamUGC`

Implements both `IWorkshopCommands` and `IWorkshopSource` in-process. This path is only valid when the **calling process itself is running under the target app** (see the routing section).

```csharp
SteamWorkshop workshop = client.GetSteamWorkshop();

RemoteStorageSubscribePublishedFileResult_t r =
    await workshop.SubscribeAsync(publishedFileId, (int)appId);
```

### Versioned interface coverage (`ISteamApps` / `ISteamClient` / `ISteamUGC`)

Each native interface is exposed as a family of versioned classes, one per Steamworks interface-version string:

| Interface | Versioned classes | Default (returned by `SteamClient`) |
| --- | --- | --- |
| `ISteamApps` | `ISteamApps001`…`ISteamApps008` | `ISteamApps008` via `GetISteamApps()` |
| `ISteamClient` | `ISteamClient007`…`ISteamClient019` | `ISteamClient017` (the entry-point client) |
| `ISteamUGC` | `ISteamUGC001`…`ISteamUGC016` (`011` omitted) | `ISteamUGC005` via `GetISteamUGC()` |

A versioned class maps only to the functions present in that version's vtable snapshot; functions a given version does not expose are declared as `[Obsolete]` stubs that throw `NotImplementedException`. Because Steamworks inserts new functions *into the middle* of these interfaces between versions, the same method sits at a different vtable index depending on the version — each versioned class therefore carries its own `[VTableIndex]` scheme, and callers must use the version that matches the interface string they requested.

Functions that need more review due to changing contracts:
* ISteamClient.CreateLocalUser (Resolved by mapping extra paramater but ignoring it)
* ISteamUGC.RequestUGCDetails (Resolved with two implementations)
* ISteamUGC.GetItemUpdateInfo (Not resolved, newer contract not implemented)
* ISteamUGC.GetItemInstallInfo (Not resolved, newer contract not implemented)
* ISteamUGC.GetQueryUGCAdditionalPreview (Not resolved, contract is highly unstable)

See [`steamworks-vtable-reference.md`](./steamworks-vtable-reference.md) for an outline of known Steamworks interfaces and their wrapping by SteamVent.

---

## The FileSystem layer (`SteamVent.FileSystem`)

Pure file-system access — no Steamworks required, so it works whether or not Steam is running.

### The strongly-typed app model — `SteamAppInfo`

`SteamAppInfoDataFile.GetApps()` parses the appcache (`appinfo.vdf`) into clean, **lossless** POCOs instead of exposing raw `BVPropertyCollection`. Each `SteamAppInfo` is a POCO pairing the *binary* chunk-header fields (`AppId`, `State`, `Checksum`, ...) with the chunk's *BVDF* payload, the `data` field, exposed as a `BvdfNode` (`app.Data`, a JObject-equivalent). The standardized `appinfo.common` / `appinfo.extended` sections are strongly-typed Bvdf nodes, and every other key is preserved (recursively) in a per-node `Extra` dictionary — so nothing Valve (or a custom appinfo entry) writes is ever dropped.

```csharp
using SteamVent.FileSystem;

foreach (SteamAppInfo app in SteamAppInfoDataFile.GetApps())
{
    // app.AppId, app.Name, app.Type ("game"/"app"/"demo"/"dlc"...), app.IconPath
    if (app.Common is { } common)
    {
        common.Developers;          // IReadOnlyList<string>
        common.Publishers;          // IReadOnlyList<string>
        common.OriginalReleaseDate; // DateTime?
        common.HasLibraryCapsule;   // bool
        common.Extra["oslist"];     // string  — non-standard keys live in the Extra dictionary
    }
    app.Data;             // BvdfNode — the `data` field, root of the BVDF tree, lossless (every key)
    app.Dependencies;     // BvdfNode — the appinfo.dependencies section
}
```

**Two halves, one record.** A `SteamAppInfo` is (a) the *binary* chunk header — plain POCO properties (`app.AppId`, `app.State`, `app.Checksum`, ...) that live in the file's binary layout, **not** in the BVDF — plus (b) the *BVDF* payload (`app.Data`, a lossless `BvdfNode` whose `appinfo` child holds `app.Common` / `app.Extended` / `app.Dependencies`), where unknown objects become child `BvdfNode`s and VDF arrays become `List<object>`. The raw `BVPropertyCollection` is still available in `SteamAppInfoDataFile.GetSteamAppInfoDataFile()`'s chunks.

**`BvdfNode` — the shared base + generic object node.** `SteamAppInfoCommon` and `SteamAppInfoExtended` derive from `BvdfNode` (they genuinely *are* the `common` / `extended` BVDF sections); `SteamAppInfo` itself is **not** a `BvdfNode` — it is a POCO that *holds* the BVDF payload as `BvdfNode`s (`app.Data`, `app.Dependencies`). `BvdfNode` owns the `Extra` dictionary and exposes a `dynamic` string-key indexer over both the elevated properties and `Extra`. A plain `BvdfNode` (no elevated keys) is the generic "unknown object" used for nested sections that are not yet standardized.

```csharp
BvdfNode node = app.Data!;
node["appinfo"]["common"]["name"]; // the [string] indexer walks elevated keys then Extra, nested nodes included
node.HasKey("oslist");             // true if the key is elevated OR present in Extra
node.MappedKeys;                   // the standardized keys this node type elevates
foreach (KeyValuePair<string, object?> kv in node.Entries())  // elevated keys, then Extra
    System.Console.WriteLine($"{kv.Key} = {kv.Value}");
```

**Elevating a key.** When a key that currently lives in `Extra` becomes standardized, add it to the node's `MappedKeys`, add a strongly-typed property, and return it from `TryGetMapped`. From that point on it no longer appears in `Extra` and the indexer returns its typed value.

**Serialization names.** Each property that maps to a single BVDF key carries a `[DataMember(Name = ...)]` (from `System.Runtime.Serialization`) whose `Name` is the *original* key — e.g. `OriginalReleaseDate` → `"original_release_date"`, `Developer` → `"developer"`, and `SteamAppInfo.Data` → `"data"`. So a contract-based serializer (e.g. `DataContractJsonSerializer`) round-trips these properties under the same key names Valve writes. The binary chunk-header fields (`AppId`, `State`, `LastUpdate`, `AccessToken`, `Checksum`, `LastChangeNumber`) are *not* BVDF keys and are unannotated, as are computed/derived properties with no single backing key (`Developers`, `Publishers`, `HasLibraryCapsule`, `Common`, `Extended`, `IconPath`, `Dependencies`, …). When you elevate a new key, add the matching `[DataMember(Name = "...")]` alongside it.
ies with no single backing key are unannotated too. When you elevate a new key, add the matching `[DataMember(Name = "...")]` alongside it.
cessInfo.GetSteamPid()`.

---

## Configuration & routing (`SteamContext`)

`SteamContext` (singleton, `SteamContext.GetInstance()`) is the public gateway to Steam. It **owns** the configuration (`ctx.Options`), the in-process Steamworks lifecycle, and the optional-module (Bridge/SteamCmd) resolution — and delegates the actual per-operation composition to the two child facades below (it also re-exposes them as thin passthroughs). Each facade deliberately composes the enabled subsystems in its own order, with its own concurrency and fallbacks, gated by `ctx.Options`, so a disabled or excised subsystem degrades the call instead of breaking it. `ctx.Options` is the settable **`SteamVentOptions`** settings object:

```csharp
using SteamVent;
using SteamVent.Core;

SteamContext ctx = SteamContext.GetInstance();

// SteamVentOptions is settable before first use. Each subsystem is an explicit enable flag:
ctx.Options.UseSteamworks = true;    // in-process Steamworks (default on): ownership, install state, in-process workshop
ctx.Options.UseBridge     = false;   // optional Bridge assembly (default off): workshop data + commands for any appid
ctx.Options.UseSteamCmd   = false;   // optional SteamCmd assembly (default off): workshop status
ctx.Options.UseFileSystem = true;    // filesystem appcache / install dirs / local workshop (default on)

// Force the in-process context to a specific appid, and have the context validate on start that it is
// actually running under it (throws if the live ISteamUtils.GetAppID differs - something got in the way):
ctx.Options.SteamworksAppId = 1091500;   // e.g. Battlezone: Redux; the live value is reported via ActiveSteamworksAppId
```

### Child service facades — `ctx.SteamApps` and `ctx.SteamWorkshop`

`SteamContext` is the single gateway to Steam. It maintains the `Steamworks` client itself (never exposed) and offers two
**latched** facades — the same instance is returned on every access until the context is disposed — each of which routes
through the sources you enabled, so you get the answer without knowing which module produced it:

- **`ctx.SteamApps`** (type `SteamVent.SteamApps`) — app queries (owned apps / install state / install dir / metadata / engine mods), composing the in-process Steamworks child with the filesystem fallback.
- **`ctx.SteamWorkshop`** (type `SteamVent.SteamWorkshop`) — workshop reads and appid-context commands (subscribe / download / unsubscribe), routed in-process when this process runs under the target app, else via the Bridge, SteamCmd, or the FileSystem fallback.

```csharp
using SteamVent;
using SteamVent.Core;

SteamContext ctx = SteamContext.GetInstance();
ctx.Options.UseBridge      = true;    // enable the optional Bridge module (in-process Steamworks is on by default)
ctx.Options.SteamworksAppId = 1091500;   // request the in-process context (Battlezone: Redux); not authoritative
UInt32 activeAppId         = ctx.ActiveSteamworksAppId;  // the appid the process actually runs under (latched live)

SteamApps apps         = ctx.SteamApps;      // latched — one instance, owned by the context
SteamWorkshop workshop = ctx.SteamWorkshop;  // latched — routes per appid

bool?   installed = apps.GetAppInstalled(appId);   // null when Steamworks is unavailable
bool?   owned     = apps.IsSubscribed(appId);
string? dir       = apps.GetAppInstallDir(appId);

await workshop.SubscribeAsync(publishedFileId, (int)appId);   // routed in-process vs Bridge vs SteamCmd
var items   = await workshop.GetInstalledWorkshopItems((int)appId);
var status  = await workshop.WorkshopStatusAsync(appId);
```

### Workshop routing: in-process (`Steamworks`) vs `Bridge`

Workshop **commands** (subscribe / unsubscribe / download) can come from two places, and the context routes per-app:

- **`Steamworks` (in-process)** — the current process must be running under the target app, so it can talk to the live Steam client directly.
- **`Bridge`** — a separate child process handles the IPC, so it works for any appid from any host.

`SteamContext` picks the right one for a given appid:

```csharp
IWorkshopCommands? cmds = ctx.GetWorkshopCommands(appId);

// Options.UseSteamworks enabled AND the LIVE appid (ctx.ActiveSteamworksAppId) == appId -> in-process SteamWorkshop
// otherwise, if Options.UseBridge is enabled and the Bridge assembly is present         -> Bridge
```

Routing is decided from the *live* appid, not the configured one: `SteamworksAppId` is only the request that established the context, so the configured value and the running process can never drift apart.

Convenience methods apply the routing for you:

```csharp
RemoteStorageSubscribePublishedFileResult_t sub = await ctx.WorkshopSubscribeAsync(publishedFileId, appId);
DownloadItemResult_t dl                         = await ctx.WorkshopDownloadAsync(publishedFileId, appId);
RemoteStorageUnsubscribePublishedFileResult_t un = await ctx.WorkshopUnsubscribeAsync(publishedFileId, appId);
```

### Enumerating owned apps

`GetOwnedApps()` reads the local appcache and intersects it with live ownership (requires Steam running):

```csharp
List<SteamLaunchableApp>? owned = ctx.GetOwnedApps();
```

`GetAppsWithMetadata(UInt64[]? appIds = null)` returns richer app metadata (developers/publishers, library-art flags, release dates).

---

## Using the Bridge directly

When you just want the process-isolated Bridge without the full context:

```csharp
using SteamVent.Bridge;
using SteamVent.Core;
using SteamVent.Core.Steamworks;

using BridgeContext bridge = new(appId);
var items = await bridge.SteamWorkshopListAsync();
var sub   = await bridge.SteamWorkshopSubscribeAsync(publishedFileId);
var dl    = await bridge.SteamWorkshopDownloadAsync(publishedFileId);
var un    = await bridge.SteamWorkshopUnsubscribeAsync(publishedFileId);
```

### Bridge diagnostics & error reporting

A failing request surfaces as the unified `WorkshopException` (the one type a caller catches), which boxes the rich `BridgeException` in it as `InnerException`; the `BridgeException` is what carries the context needed to diagnose the failure without reaching for the logs. The **message itself** is self-describing — e.g. `[workshop download 3718541406, app 624970, bridge error] System.InvalidOperationException: DownloadItem(3718541406) failed before completing (item state=[Subscribed|NeedsUpdate], download=0/0 bytes).` — so it stays informative even in runners (like xUnit) that only print the message. The `BridgeException` exposes these fields for programmatic access, and its `ToString()` appends the deeper context:

- `Command` / `CallbackId` / `AppId` — what was in flight;
- `Reason` — how the failure was detected: `bridge error`, `timeout`, `process exit`, `malformed response`, or `decode failure`;
- `ExitCode` — the bridge process exit code (for `process exit`);
- `RawResponse` — the raw envelope/payload the bridge sent (when one was received);
- `BridgeExceptionType` / `BridgeStackTrace` — the exception the *bridge process* threw (for `bridge error`);
- `BridgeOutput` — the recent tail of bridge diagnostics: `progress` updates (e.g. download `state` + `bytesDownloaded` / `bytesTotal`), `stderr`, and any non-protocol stdout;
- **Item state** — for workshop commands the bridge additionally captures the *live* Steam state of the item at the moment of failure and embeds it in the message (e.g. `item state=[Subscribed|NeedsUpdate], download=0/0 bytes`). These come from `GetItemState` / `GetItemDownloadInfo` / `GetItemInstallInfo`, which are synchronous local queries and keep working even when the async Steam call itself reports a failure. How to read it: `state=[none]` → the subscribe had not yet applied (a subscribe→download race); `Subscribed|NeedsUpdate` → Steam accepted the subscription but rejected the download call (transient / Steam-side); `Subscribed|Installed` → it actually landed (a timing issue in the completion check); a non-zero `download=X/Y` → the transfer started and aborted.

`progress` lines and `stderr` are always routed to the `BridgeOutput` event (`EventHandler<string>`), so a host can log them live; the client also retains the most recent few hundred lines so they ride along on the next exception.

The wire protocol's `error` envelope is `{ "type": "error", "message", "errorType", "stackTrace" }` — `message` is the human summary, while `errorType` / `stackTrace` carry the bridge-side exception for the failure.

Design intent: the bridge stays deliberately **thin** — it does not retry, gate, or otherwise "fix" a failed Steam call. It reports the failure (with the item-state detail above) and leaves the decision to invoke again to the caller. This is what keeps the in-process `Steamworks` path and the `Bridge` path behaviourally equivalent: both implement the same `IWorkshopCommands` / `IWorkshopSource` contract (`SteamVent.Core`), so a caller can rely on one API surface without knowing which backend served the call.

### Downloads complete when they're actually done

`DownloadAsync` on **both** backends honors the same contract: the call completes only when the item is **fully installed and up to date** (`Installed && !NeedsUpdate`) — not when Steam merely acknowledges the `DownloadItem` request. So neither the in-process `Steamworks` path nor the `Bridge` path can report a premature "done"; a failed or stalled transfer surfaces as an exception carrying the live item state (`state=[...], download=X/Y bytes`), or the caller cancels it via `CancellationToken`.

Both also report live progress through `IProgress<WorkshopItemProgress>` (state + bytes + `Percent`), throttled to change, so a caller can drive a progress bar without knowing which backend answered:

```csharp
IProgress<WorkshopItemProgress> progress = new(sample =>
    System.Console.WriteLine($"  {sample.Percent:0.#}%  {sample.BytesDownloaded}/{sample.BytesTotal}"));

await ctx.SteamWorkshop.WorkshopDownloadAsync(publishedFileId, appId, progress);
```

### Bridge lifetime (per-AppId pool)

A `BridgeContext` is a child process scoped to one AppID. Rather than spawn one per command, `SteamVent.Bridge.BridgePool` keeps a **warm process per AppID**:

- Repeated work on the same AppID **reuses the same child process** (no per-command spawn); reservations are reference-counted so concurrent commands on one AppID share it safely.
- Different AppIDs run **concurrently**, each with its own child process — work on two AppIDs at once yields two live bridges.
- Each entry **decays independently** once idle for `BridgePool.IdleTimeout` (default **30s**). Keep it short: an idle bridge still holds a Steam app-context alive (it can make Steam treat that app as running), so a long hang time is actively harmful. Tune it via `SteamVentOptions.BridgeIdleTimeout` or `BridgePool.IdleTimeout`.

The adaptive `BridgeSource` routes through the shared `BridgePool.Default`; direct users can drive the pool too:

```csharp
using BridgePool pool = new(TimeSpan.FromSeconds(15));
BridgeContext bridge = pool.Acquire(appId);
try
{
    await bridge.SubscribeAsync(publishedFileId, (int)appId);
    await bridge.DownloadAsync(publishedFileId, (int)appId, progress);
}
finally
{
    pool.Release(appId); // or simply let the pool reap it after IdleTimeout
}
```

---

## Workshop exceptions & AppID context

Workshop operations (subscribe / download / unsubscribe, and the AppID-context checks around them) surface a single, unified **`WorkshopException`** (`SteamVent.Core`) no matter which backend answered, so a caller catches one type:

```csharp
try
{
    await ctx.SteamWorkshop.WorkshopDownloadAsync(publishedFileId, appId, progress);
}
catch (WorkshopException ex) when (ex.IsAppIdMismatch)
{
    // Steam is not actually running under the requested AppID (a stale steam_appid.txt, another game, ...).
    // ex.AppId is the requested AppID; ex.Message explains the mismatch.
}
catch (WorkshopException ex)
{
    // Any other workshop failure. For the Bridge, ex.InnerException is the rich BridgeException.
}
```

- **In-process `Steamworks`** throws `WorkshopException` directly (it replaces the former thin `SteamException` on the workshop command path).
- **Bridge** boxes its rich `BridgeException` in the `WorkshopException` as `InnerException`, so the bridge diagnostics (raw response, stderr tail, item state) stay reachable.

**AppID-context affirmation.** The AppID context is fixed once the Steamworks context starts, so both backends affirm it **once, at startup** — not on every command — and the check is **Steamworks/Bridge-wide** (it guards the whole context, not just the workshop commands):

- **In-process `Steamworks`** affirms it in `SteamClient`, right after the context starts, from `SteamVentOptions.SteamworksAppId` (a nullable three-state):
  - `null` (unset) → **permissive**: the context is whatever Steam reports (a host that is not itself a Steam game, or one that simply wants its own context). No check.
  - `0` or `N` (set) → the context forces that AppID (`SteamAppId` env var), then confirms the live `ISteamUtils.GetAppID()` equals it. A mismatch throws a `WorkshopException` with `IsAppIdMismatch = true` (a hard error — the context is never latched under an AppID it was not actually asked for).
- **Bridge** affirms it once in the child process at startup (a single `ISteamUtils.GetAppID()` query, then cached). A bridge is always scoped to exactly one app, so the live AppID must equal that scope; if it does not, every command reports the cached mismatch (`IsAppIdMismatch = true`) instead of acting on the wrong app's workshop.

Because the affirmation runs once at context start rather than per command, the expensive live query happens a single time, while the per-command cost is a cheap cached check (Bridge) or nothing (in-process).

This catches the "sleeping appid" case where a stale `steam_appid.txt` (or another game) overrides the forced context and would otherwise silently redirect the work to the wrong app's workshop.

## Building

The repo mixes two toolchains, so building is split between the native C++ subprojects and the .NET side:

- **Native C++ (x64-only)** — `steamcmdinj.dll` and `steamcmdprox.exe` build with MSBuild from Visual Studio. The `dotnet` CLI **cannot** build them (it lacks the C++ targets — error `MSB4278`).
- **.NET (`net10.0`)** — everything else builds with the .NET SDK. `SteamVent.SteamCmd` copies the two native artifacts out of `<project>/bin/<Configuration>/` during its post-build step, so build the C++ side first.

`.vscode/tasks.json` provides the everyday workflow (Command Palette → **Terminal: Run Task**):

| Task | Default | What it does |
| --- | --- | --- |
| **Build Solution** | ✔ build | `MSBuild` on `SteamVent.sln` — the whole solution (C++ + .NET) in dependency order. |
| **Build C++ Subprojects** | | Builds both native x64 projects (in parallel). |
| **Build .NET** | | A .NET-only rebuild (anchored on `SteamVent.Tests`, which pulls the full .NET graph) after the C++ side. |
| **Test** | ✔ test | `dotnet test` on `SteamVent.Tests` (see *Testing* below). |

> MSBuild is invoked by its full path because `msbuild` is not on `PATH`. If your Visual Studio install changes, update the `MSBuild.exe` path in `.vscode/tasks.json` (or add `msbuild` to `PATH` and switch the tasks to it).

## Testing

`SteamVent.Tests` (xUnit, `net10.0`) covers the Core abstractions, the `BVdf` reader, the result-struct round-trips, the versioned `ISteamApps` interop, and a live **Bridge** workshop subscribe/download/unsubscribe round-trip.

**Vtable audit.** A signature-aware auditor (`references/_sv_audit.ps1`) cross-checks every versioned `ISteam*` `[VTableIndex]` against the official Steamworks SDK per-version headers (`references/steamworks_sdk`, resolved by git tag to the release carrying each interface version), falling back to the Goldberg Emulator SDK (`references/goldberg_emulator/sdk_includes/isteam*NNN.h`) for the five snapshots the official repo does not tag (`ISteamUGC004`/`005`/`006`, `ISteamClient013`/`014`). It compares **slot-for-slot on parameter shape** (count + param names), applying the game-build convention (`#ifdef _PS3` excluded, `STEAM_PRIVATE_API` included) and stripping `[In, Out]` / `STEAM_OUT_STRING_COUNT( … )` annotations — so C++ overloads and `[In, Out]` array marshalling are disambiguated instead of raising false positives. Each interface reports `OK / SOFT / DIFF` (plus `ref-only`/`our-only` gaps).

`ISteamUtilsTests` exercises `ISteamUtils.GetAppID` (the source of truth for the active appid) and `SteamClient.AppId`, which latches that value once the Steamworks context starts and is then read-only. `ISteamAppsTests` covers the versioned `BIsAppInstalled` interop, reading `SteamAppsTests:InstalledAppID` / `SteamAppsTests:UninstalledAppID` from configuration.

The Bridge round-trip test reads `BridgeTests:AppID` and `BridgeTests:PublishedFileID` from configuration and requires a **running Steam session actually under the configured app context** — swap in a valid item ID before running it. When Steam is not running under that app (e.g. the shell / a different app, or an offline environment), it **skips**: the AppID-context guard now fails loudly with a `WorkshopException` (`IsAppIdMismatch`) instead of silently acting on the wrong app's workshop. When it fails for another reason, the thrown `WorkshopException` boxes the rich `BridgeException` (command, app ID, reason, raw response, captured bridge `progress`/`stderr` tail) as `InnerException` (see "Bridge diagnostics & error reporting"). `WorkshopExceptionTests` covers the unified exception (the AppID-mismatch factory, the contextual `ToString`, and the boxed-`InnerException` contract) without needing Steam.

All the interproc tests (`ISteamAppsTests`, `ISteamUtilsTests`) share the `SteamInterProc` collection (serialized — the Steam client is a process-wide singleton) and a `SteamTestGuard` that fails fast if a local `steam_appid.txt` is present in the test working directory: such a file would silently override the environment-variable appid context (`SteamClient.AppId`) and make the results non-deterministic. The interproc tests require a **running Steam client**.

```
dotnet test SteamVent.Tests\SteamVent.Tests.csproj
```
