# SteamVent
A .NET library for interfacing with the Steam client application via IPC (Steamworks interproc), the registry, and file-system access to the local Steam installation.

SteamVent is the buisness logic for the **BZR Mod Manager**. It is organized into three assemblies:

| Project | Target | Purpose |
| --- | --- | --- |
| `SteamVent.Core` | `net10.0` | Framework-agnostic abstractions: `PublishedFileData`, `WorkshopItemStatus`, the workshop `IWorkshopSource` / `IWorkshopCommands` contracts, `SteamVentOptions` (per-subsystem enable flags + `SteamworksAppId`), the `BVdf` binary-VDF reader, and the Steam result structs (`DownloadItemResult_t`, `RemoteStorage*Result_t`, `EItemState`, `EResult`, `SteamTypes`). |
| `SteamVent.Bridge` | `net10.0` | The process-isolated IPC **Bridge** (`BridgeContext` + the `ISteamApps`/`ISteamUtils`/`ISteamUGC` command wrappers and their generated proxies). |
| `SteamVent` | `net10.0` | The main library: the `Steamworks` (native interproc) layer, the `FileSystem` layer, and the `SteamContext` facade, which deliberately composes whichever subsystems are enabled per operation. |

> The two C++ projects (`steamcmdinj`, `steamcmdprox`) implement the `steamcmd` injection/proxy used by the Bridge child process and build with MSBuild, not the `dotnet` CLI.

---

## Project structure

```
SteamVent/
├── SteamVent.Core/              # net10.0
│   ├── Source.cs                # Source, PublishedFileData, IWorkshopSource, IWorkshopCommands
│   ├── BVdf.cs                  # Binary VDF reader (BVdf, BVProperty, BVPropertyCollection, ...)
│   └── InterProc/               # Steam IPC result enums/structs
│       ├── SteamTypes.cs        # EResult, CallResult<T>, ISteamAPICall
│       ├── EItemState.cs        # EItemState / EItemPreview flags
│       ├── DownloadItemResult_t.cs
│       ├── RemoteStorageSubscribePublishedFileResult_t.cs
│       └── RemoteStorageUnsubscribePublishedFileResult_t.cs
├── SteamVent.Bridge/            # net10.0, depends on Core
│   ├── BridgeContext.cs         # IPC client: launches a Bridge child process, sends commands
│   ├── BridgeSource.cs          # IWorkshopSource / IWorkshopCommands over the Bridge
│   ├── Interfaces/              # ISteamApps / ISteamUtils / ISteamUGC command contracts
│   └── InterProc/               # SteamAPICall_t, Callback<T>, generated proxies + result structs
└── SteamVent/                   # net10.0, depends on Core
    ├── SteamContext.cs          # Public facade / gateway: per-operation composition of the enabled subsystems
    ├── SteamApps.cs             # SteamVent app facade — a box over Steamworks.SteamApps (ctx.SteamApps)
    ├── SteamWorkshop.cs         # SteamVent workshop facade — routes reads/commands in-process, Bridge, SteamCmd, or disk (ctx.SteamWorkshop)
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
- **`IWorkshopSource`** — read-side workshop listing:
  ```csharp
  Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken ct = default);
  Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken ct = default);
  ```
- **`IWorkshopCommands`** — command-side workshop operations:
  ```csharp
  Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeAsync(ulong publishedFileId, int appid, CancellationToken ct = default);
  Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken ct = default);
  Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, CancellationToken ct = default);
  ```
- **`BVdf`** — a reader for Valve's binary `.vdf` format (the appcache `appinfo.vdf`), exposing `BVPropertyCollection` / `BVProperty` with typed `GetValue<T>()` accessors.

---

## The Steamworks layer (`SteamVent.Steamworks`)

This is the in-process, native-interproc layer (renamed from the former `SteamVent.InterProc` namespace to `SteamVent.Steamworks` / `SteamVent.Steamworks.Native`).

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

---

## The FileSystem layer (`SteamVent.FileSystem`)

Pure file-system access — no Steamworks required, so it works whether or not Steam is running.

### The strongly-typed app model — `SteamAppInfo`

`SteamAppInfoDataFile.GetApps()` parses the appcache (`appinfo.vdf`) into clean, **lossless** POCOs instead of exposing raw `BVPropertyCollection`. Every key of a VDF section is reachable: keys that are standardized become strongly-typed properties, and the rest are preserved (recursively) in a per-node `Extra` dictionary — so nothing Valve (or a custom appinfo entry) writes is ever dropped.

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
    app.AppInfo;          // BvdfNode — the whole appinfo section, lossless (every key)
    app.Dependencies;     // BvdfNode — the appinfo.dependencies section
}
```

**Three complementary forms.** The same record is available as (a) strongly-typed properties (`app.AppId`, `app.Common.Name`), (b) a complete generic node (`app.AppInfo`, `app.Common.Extra`, `app.Dependencies`) where unknown objects become child `BvdfNode`s and VDF arrays become `List<object>`, and (c) the raw `BVPropertyCollection` in `SteamAppInfoDataFile.GetSteamAppInfoDataFile()`'s chunks.

**`BvdfNode` — the shared base + generic object node.** `SteamAppInfo`, `SteamAppInfoCommon` and `SteamAppInfoExtended` all derive from `BvdfNode`, which owns the `Extra` dictionary and exposes a `dynamic` string-key indexer over both the elevated properties and `Extra`. A plain `BvdfNode` (no elevated keys) is the generic "unknown object" used for nested sections that are not yet standardized.

```csharp
BvdfNode node = app.AppInfo!;
node["common"]["name"];            // the [string] indexer walks elevated keys then Extra, nested nodes included
node.HasKey("oslist");             // true if the key is elevated OR present in Extra
node.MappedKeys;                   // the standardized keys this node type elevates
foreach (KeyValuePair<string, object?> kv in node.Entries())  // elevated keys, then Extra
    System.Console.WriteLine($"{kv.Key} = {kv.Value}");
```

**Elevating a key.** When a key that currently lives in `Extra` becomes standardized, add it to the node's `MappedKeys`, add a strongly-typed property, and return it from `TryGetMapped`. From that point on it no longer appears in `Extra` and the indexer returns its typed value.

Other helpers: `SteamAppInfoDataFile.GetSteamAppInfoDataFile(path?)` (raw chunks), `SteamProcessInfo.GetSteamInstallPath()`, `SteamProcessInfo.GetSteamLibraryPaths()`, `SteamProcessInfo.GetSteamPid()`.

---

## Configuration & routing (`SteamContext`)

`SteamContext` (singleton, `SteamContext.GetInstance()`) is the public facade and the single gateway to Steam. It is **tightly coupled** to its subsystems: each top-level method deliberately composes the specific children it wants — in its own order, with its own concurrency and fallbacks — gated by `ctx.Options`, so a disabled or excised subsystem degrades the call instead of breaking it. `ctx.Options` is the settable **`SteamVentOptions`** settings object:

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

- **`ctx.SteamApps`** (type `SteamVent.SteamApps`) — app queries (ownership / install state / install dir), over the in-process Steamworks child.
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

---

## Testing

`SteamVent.Tests` (xUnit, `net10.0`) covers the Core abstractions, the `BVdf` reader, the result-struct round-trips, the versioned `ISteamApps` interop, and a live **Bridge** workshop subscribe/download/unsubscribe round-trip.

`ISteamUtilsTests` exercises `ISteamUtils.GetAppID` (the source of truth for the active appid) and `SteamClient.AppId`, which latches that value once the Steamworks context starts and is then read-only. `ISteamAppsTests` covers the versioned `BIsAppInstalled` interop, reading `SteamAppsTests:InstalledAppID` / `SteamAppsTests:UninstalledAppID` from configuration.

The Bridge round-trip test reads `BridgeTests:AppID` and `BridgeTests:PublishedFileID` from configuration and requires a **running Steam session with a real workshop item** — swap in a valid item ID before running it. In an offline/headless environment it is expected to fail/skip.

All the interproc tests (`ISteamAppsTests`, `ISteamUtilsTests`) share the `SteamInterProc` collection (serialized — the Steam client is a process-wide singleton) and a `SteamTestGuard` that fails fast if a local `steam_appid.txt` is present in the test working directory: such a file would silently override the environment-variable appid context (`SteamClient.AppId`) and make the results non-deterministic. The interproc tests require a **running Steam client**.

```
dotnet test Deps\SteamVent\SteamVent.Tests\SteamVent.Tests.csproj
```
