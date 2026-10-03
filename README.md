# SteamVent
A .NET library for interfacing with the Steam client application via IPC (Steamworks interproc), the registry, and file-system access to the local Steam installation.

SteamVent is the "eat your own dogfood" host for the **BZR Mod Manager** (`BZRNetMonitor` service). It is organized into three assemblies:

| Project | Target | Purpose |
| --- | --- | --- |
| `SteamVent.Core` | `netstandard2.0` | Framework-agnostic abstractions: `Source`, `PublishedFileData`, the workshop `IWorkshopSource` / `IWorkshopCommands` contracts, the `BVdf` binary-VDF reader, and the Steam result structs (`DownloadItemResult_t`, `RemoteStorage*Result_t`, `EItemState`, `EResult`, `SteamTypes`). |
| `SteamVent.Bridge` | `netstandard2.0` | The process-isolated IPC **Bridge** (`BridgeContext` + the `ISteamApps`/`ISteamUtils`/`ISteamUGC` command wrappers and their generated proxies). |
| `SteamVent` | `net10.0` | The main library: the `Steamworks` (native interproc) layer, the `FileSystem` layer, the `Sources` (Bridge / filesystem / Steamworks), `SteamVentOptions`, and the `SteamContext` facade. |

> The two C++ projects (`steamcmdinj`, `steamcmdprox`) implement the `steamcmd` injection/proxy used by the Bridge child process and build with MSBuild, not the `dotnet` CLI.

---

## Project structure

```
SteamVent/
├── SteamVent.Core/              # netstandard2.0
│   ├── Source.cs                # Source, PublishedFileData, IWorkshopSource, IWorkshopCommands
│   ├── BVdf.cs                  # Binary VDF reader (BVdf, BVProperty, BVPropertyCollection, ...)
│   └── InterProc/               # Steam IPC result enums/structs
│       ├── SteamTypes.cs        # EResult, CallResult<T>, ISteamAPICall
│       ├── EItemState.cs        # EItemState / EItemPreview flags
│       ├── DownloadItemResult_t.cs
│       ├── RemoteStorageSubscribePublishedFileResult_t.cs
│       └── RemoteStorageUnsubscribePublishedFileResult_t.cs
├── SteamVent.Bridge/            # netstandard2.0, depends on Core
│   ├── BridgeContext.cs         # IPC client: launches a Bridge child process, sends commands
│   ├── BridgeSource.cs          # IWorkshopSource / IWorkshopCommands over the Bridge
│   ├── Interfaces/              # ISteamApps / ISteamUtils / ISteamUGC command contracts
│   └── InterProc/               # SteamAPICall_t, Callback<T>, generated proxies + result structs
└── SteamVent/                   # net10.0, depends on Core + Bridge
    ├── SteamContext.cs          # Public facade: options, pipeline, sources, routing
    ├── SteamVentOptions.cs      # Source selection, SteamworksAppId, Bridge command config
    ├── SteamLaunchable.cs       # SteamLaunchable* POCOs returned by the context
    ├── Steamworks/              # In-process Steamworks layer (namespace SteamVent.Steamworks)
    │   ├── SteamClient.cs       #   Owned lifecycle: pipe/user + interop interface factory + thin wrappers
    │   ├── SteamApps.cs         #   Thin, non-throwing wrapper over ISteamApps (ownership / install dir)
    │   ├── SteamWorkshop.cs     #   Thin wrapper over ISteamUGC (in-process IWorkshopCommands/IWorkshopSource)
    │   └── Native/              # namespace SteamVent.Steamworks.Native
    │       ├── Steam.cs         #   Steam interop helper (CreateInterface / GetAPICallResult / ...)
    │       ├── Attributes/      #   [InterfaceVersion], [VTableIndex]
    │       ├── Interfaces/      #   ISteamApps(+versioned), ISteamClient, ISteamUGC, ISteamUtils, CSteamID
    │       ├── Interop/         #   SteamInterfaceWrapper
    │       └── Native/          #   SteamNative, SysNative
    ├── FileSystem/              # File-system access to a local Steam install (namespace SteamVent.FileSystem)
    │   ├── SteamAppInfoDataFile.cs  # appcache (appinfo.vdf) parser + GetApps()
    │   ├── SteamAppInfo.cs          # strongly-typed per-app model (SteamAppInfo / Common / Extended)
    │   ├── SteamProcessInfo.cs      # Steam install path, library paths, running-process detection
    │   └── SteamWorkshop.cs         # Read workshop items straight from the workshop/contentdl cache
    └── Sources/
        ├── BuiltinAppSources.cs     # SteamworksAppSource + FileSystemAppSource
        └── FileSystemWorkshopSource.cs
```

---

## Core abstractions (`SteamVent.Core`)

- **`Source`** — a self-describing, optionally async source of data. Subclass it, name it, optionally set `DefaultPollInterval`, and provide sync `IReadOnlyCollection<T> Get<T>()` and/or async `Task<IReadOnlyCollection<T>> GetAsync<T>(CancellationToken)`.
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

`SteamAppInfoDataFile.GetApps()` parses the appcache (`appinfo.vdf`) into clean POCOs instead of exposing raw `BVPropertyCollection`:

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
    }
}
```

Other helpers: `SteamAppInfoDataFile.GetSteamAppInfoDataFile(path?)` (raw chunks), `SteamProcessInfo.GetSteamInstallPath()`, `SteamProcessInfo.GetSteamLibraryPaths()`, `SteamProcessInfo.GetSteamPid()`.

---

## Configuration & routing (`SteamContext`)

`SteamContext` (singleton, `SteamContext.GetInstance()`) is the public facade. `ctx.Options` exposes the shared, mutable **`SteamVentOptions`** settings object:

```csharp
using SteamVent;
using SteamVent.Core;

SteamContext ctx = SteamContext.GetInstance();

ctx.Options.Sources = SteamSource.Bridge | SteamSource.Steamworks;  // which sources to include
ctx.Options.SteamworksAppId = 1091500;   // required when SteamSource.Steamworks is set (Battlezone: Redux)
ctx.Options.BridgeCommand = "dotnet";    // optional Bridge child-process overrides
// ctx.Options.BridgeCommandArgs = new() { "SteamVentContextBridge.dll" };
// ctx.Options.BridgeWorkingDirectory = @"C:\SteamVentBridge";

IReadOnlyList<ISource> active  = ctx.Sources;   // the enabled sources
SourcePipeline pipeline        = ctx.Pipeline;  // the merged / filtered source pipeline
```

### Workshop routing: in-process (`Steamworks`) vs `Bridge`

Workshop **commands** (subscribe / unsubscribe / download) can come from two places, and the context routes per-app:

- **`Steamworks` (in-process)** — the current process must be running under the target app, so it can talk to the live Steam client directly.
- **`Bridge`** — a separate child process handles the IPC, so it works for any appid from any host.

`SteamContext` picks the right one for a given appid:

```csharp
IWorkshopCommands? cmds = ctx.GetWorkshopCommands(appId);

// SteamSource.Steamworks enabled AND Options.SteamworksAppId == appId  -> in-process SteamWorkshop
// otherwise, if SteamSource.Bridge is enabled                          -> Bridge
```

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

The Bridge round-trip test reads `BridgeTests:AppID` and `BridgeTests:PublishedFileID` from configuration and requires a **running Steam session with a real workshop item** — swap in a valid item ID before running it. In an offline/headless environment it is expected to fail/skip.

```
dotnet test Deps\SteamVent\SteamVent.Tests\SteamVent.Tests.csproj
```
