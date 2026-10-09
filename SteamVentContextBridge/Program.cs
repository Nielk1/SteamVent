using SteamVent.Bridge;
using SteamVent.Core;
using SteamVent.Core.Steamworks;
using SteamVent.Steamworks.Native;
using SteamVent.Steamworks.Native.Interfaces;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamVentContextBridge;

/// <summary>
/// A thin, child-process friendly wrapper over the local Steam client ("SteamVent").
/// <para>
/// The wire protocol is one message per line, correlated by a callback ID, so any number of
/// commands may be in flight at the same time:
/// </para>
/// <code>
///   request : "&lt;callbackId&gt; &lt;command&gt; [args...]"
///   response: "&lt;callbackId&gt;\t{ "type": "result",   "payload": &lt;json&gt; }"
///             "&lt;callbackId&gt;\t{ "type": "progress", "payload": &lt;json&gt; }"
///             "&lt;callbackId&gt;\t{ "type": "error",    "message": "&lt;text&gt;" }"
/// </code>
/// <para>
/// <c>progress</c> lines are mid-work updates that precede the final <c>result</c>/<c>error</c>
/// line of the same callback ID. Commands currently supported:
/// </para>
/// <code>
///   workshop subscribe   &lt;publishedFileId&gt;
///   workshop download    &lt;publishedFileId&gt;
///   workshop unsubscribe &lt;publishedFileId&gt;
///   workshop list
///   exit
/// </code>
/// </summary>
internal static class Program
{
    private static readonly int PollIntervalMs = 100;
    private static readonly TimeSpan DownloadDeadline = TimeSpan.FromMinutes(10);

    private static readonly SemaphoreSlim OutputLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static ISteamClient SteamClient = null!;
    private static int Pipe;
    private static int User;
    private static ISteamUtils SteamUtils = null!;
    private static ISteamUGC SteamUGC = null!;

    // The AppID this child was launched for (a bridge is scoped to exactly one app). Every workshop command verifies
    // the live Steam context against this, so a stale steam_appid.txt cannot silently redirect the work to a wrong app.
    private static UInt32 RequestedAppId;

    private static async Task<int> Main(string[] args)
    {
        if (args.Length != 1 || !UInt32.TryParse(args[0], out UInt32 appId) || appId == 0)
        {
            Console.Error.WriteLine("Usage: SteamVentContextBridge <AppId>");
            return 1;
        }

        // Scope the workshop calls to the requested app without needing a steam_appid.txt.
        RequestedAppId = appId;
        Environment.SetEnvironmentVariable("StickySteamAppId", null);
        Environment.SetEnvironmentVariable("SteamAppId", appId.ToString(CultureInfo.InvariantCulture));

        if (!Steam.Load())
        {
            Console.Error.WriteLine("Failed to load the Steam Client interface; is Steam running and logged on?");
            return 1;
        }

        SteamClient = Steam.CreateInterface<ISteamClient017>()
            ?? throw new InvalidOperationException("Steam CreateInterface(ISteamClient017) failed.");
        Pipe = SteamClient.CreateSteamPipe();
        User = SteamClient.ConnectToGlobalUser(Pipe);
        SteamUtils = SteamClient.GetISteamUtils<ISteamUtils007>(Pipe)
            ?? throw new InvalidOperationException("Steam GetISteamUtils failed.");
        SteamUGC = SteamClient.GetISteamUGC<ISteamUGC005>(User, Pipe)
            ?? throw new InvalidOperationException("Steam GetISteamUGC failed.");

        try
        {
            while (await Console.In.ReadLineAsync() is { } line)
            {
                line = line.Trim();
                if (line.Length == 0)
                    continue;

                string[] parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0 || parts[0] == "exit")
                    break;

                if (parts.Length < 2)
                {
                    await RespondErrorAsync(parts[0], "A request needs a command.");
                    continue;
                }

                string callbackId = parts[0];
                string command = parts[1];
                _ = Task.Run(() => HandleCommandAsync(callbackId, command)); // commands run concurrently
            }
        }
        finally
        {
            SteamClient.ReleaseUser(Pipe, User);
            SteamClient.BReleaseSteamPipe(Pipe);
        }

        return 0;
    }

    private static async Task HandleCommandAsync(string callbackId, string command)
    {
        string[] parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        try
        {
            switch (parts[0])
            {
                case "workshop" when parts.Length == 2:
                    await WorkshopAsync(callbackId, parts[1]);
                    break;

                default:
                    await RespondErrorAsync(callbackId, $"Unknown command: {command}");
                    break;
            }
        }
        catch (Exception ex)
        {
            await RespondErrorAsync(callbackId, ex);
        }
    }

    /// <summary>
    /// Guard: this bridge was launched to operate under a specific AppID, so verify the live Steam context is
    /// actually running under it. When a stale <c>steam_appid.txt</c> (or another game) overrides the forced context,
    /// <c>ISteamUtils.GetAppID</c> reports a different AppID and we must fail loudly rather than act on the wrong
    /// app's workshop.
    /// </summary>
    private static void EnsureAppIdContext()
    {
        if (RequestedAppId == 0)
            return;

        UInt32 live = SteamUtils.GetAppID();
        if (live != RequestedAppId)
            throw WorkshopException.AppIdMismatch(RequestedAppId, live, "workshop");
    }

    private static async Task WorkshopAsync(string callbackId, string args)
    {
        EnsureAppIdContext();
        string[] parts = args.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        switch (parts[0])
        {
            case "subscribe" or "unsubscribe" or "download"
                when parts.Length == 2 && ulong.TryParse(parts[1], out ulong publishedFileId):
            {
                switch (parts[0])
                {
                    case "subscribe":
                        await RunCallAsync<RemoteStorageSubscribePublishedFileResult_t>(
                            callbackId, SteamUGC.SubscribeItem(publishedFileId), RemoteStorageSubscribePublishedFileResult_t.k_iCallback, publishedFileId);
                        break;

                    case "unsubscribe":
                        await RunCallAsync<RemoteStorageUnsubscribePublishedFileResult_t>(
                            callbackId, SteamUGC.UnsubscribeItem(publishedFileId), RemoteStorageUnsubscribePublishedFileResult_t.k_iCallback, publishedFileId);
                        break;

                    default: // download
                        await DownloadItemAsync(callbackId, publishedFileId);
                        break;
                }
                break;
            }

            case "subscribe" or "unsubscribe" or "download":
                await RespondErrorAsync(callbackId, $"'workshop {parts[0]}' requires a <publishedFileId> argument.");
                break;

            case "list":
                await ListAsync(callbackId);
                break;

            default:
                await RespondErrorAsync(callbackId, $"Unknown workshop command: {args}");
                break;
        }
    }

    private static async Task DownloadItemAsync(string callbackId, ulong publishedFileId)
    {
        if (IsInstalledAndCurrent(SteamUGC.GetItemState(publishedFileId)))
        {
            // Already fully installed; nothing to download.
            await RespondResultAsync(callbackId, new DownloadItemResult_t
            {
                m_eResult = EResult.k_EResultOK,
                m_nPublishedFileId = publishedFileId,
            });
            return;
        }

        UInt64 call = SteamUGC.DownloadItem(publishedFileId);
        DownloadItemResult_t? maybeResult = await WaitForCallAsync<DownloadItemResult_t>(call, DownloadItemResult_t.k_iCallback);

        // The acknowledgement is the fast path. A transiently-reported failure (e.g. the item is already
        // DownloadPending because the subscription just applied) is not a dead end: if a download is actually in
        // flight we rely on the *installed* end-state below rather than the ACK, so the caller sees a clean
        // "started, then done" instead of a spurious failure.
        if (maybeResult is null || maybeResult.Value.m_eResult != EResult.k_EResultOK)
        {
            if (!IsDownloadActive(SteamUGC.GetItemState(publishedFileId)))
            {
                string detail = maybeResult is { } r ? $"returned {r.m_eResult}" : "failed before completing";
                throw new InvalidOperationException($"DownloadItem({publishedFileId}) {detail} (item {DescribeWorkshopItem(publishedFileId)}).");
            }
        }

        DownloadItemResult_t result = maybeResult
            ?? new DownloadItemResult_t { m_eResult = EResult.k_EResultOK, m_nPublishedFileId = publishedFileId };

        DateTime deadline = DateTime.UtcNow + DownloadDeadline;
        while (true)
        {
            EItemState state = SteamUGC.GetItemState(publishedFileId);

            if (IsInstalledAndCurrent(state))
            {
                await RespondResultAsync(callbackId, result);
                return;
            }

            if (!IsDownloadActive(state))
                throw new InvalidOperationException($"The download for {publishedFileId} stopped before completing (item state: {state}).");

            UInt64 bytesDownloaded = 0;
            UInt64 bytesTotal = 0;
            SteamUGC.GetItemDownloadInfo(publishedFileId, ref bytesDownloaded, ref bytesTotal);
            await RespondProgressAsync(callbackId, new { state, bytesDownloaded, bytesTotal });

            if (DateTime.UtcNow > deadline)
                throw new TimeoutException($"Timed out waiting for the download of {publishedFileId} to finish.");

            await Task.Delay(500);
        }
    }

    private static bool IsInstalledAndCurrent(EItemState state)
        => state.HasFlag(EItemState.k_EItemStateInstalled) && !state.HasFlag(EItemState.k_EItemStateNeedsUpdate);

    private static bool IsDownloadActive(EItemState state)
        => state.HasFlag(EItemState.k_EItemStateDownloading) || state.HasFlag(EItemState.k_EItemStateDownloadPending);

    private static async Task ListAsync(string callbackId)
    {
        List<PublishedFileData> items = new();

        UInt32 count = SteamUGC.GetNumSubscribedItems();
        UInt64[] publishedFileIds = new UInt64[count];
        count = SteamUGC.GetSubscribedItems(publishedFileIds, count);

        StringBuilder folder = new(1024);
        for (UInt32 i = 0; i < count; i++)
        {
            UInt64 publishedFileId = publishedFileIds[i];
            EItemState state = SteamUGC.GetItemState(publishedFileId);

            PublishedFileData item = new(publishedFileId, state);
            if (state.HasFlag(EItemState.k_EItemStateInstalled))
            {
                UInt64 sizeOnDisk = 0;
                UInt32 timeStamp = 0;
                if (SteamUGC.GetItemInstallInfo(publishedFileId, ref sizeOnDisk, folder, (UInt32)folder.Capacity, ref timeStamp))
                {
                    item.SizeOnDisk = sizeOnDisk;
                    item.Folder = folder.ToString();
                    item.TimeStamp = timeStamp;
                }
            }

            if (state.HasFlag(EItemState.k_EItemStateNeedsUpdate))
            {
                UInt64 bytesDownloaded = 0;
                UInt64 bytesTotal = 0;
                if (SteamUGC.GetItemDownloadInfo(publishedFileId, ref bytesDownloaded, ref bytesTotal))
                {
                    item.BytesDownloaded = bytesDownloaded;
                    item.BytesTotal = bytesTotal;
                }
            }

            items.Add(item);
        }

        await RespondResultAsync(callbackId, items);
    }

    private static async Task RunCallAsync<T>(string callbackId, UInt64 call, int k_iCallback, ulong? publishedFileId = null) where T : unmanaged
    {
        T? result = await WaitForCallAsync<T>(call, k_iCallback);
        if (result is null)
        {
            string item = publishedFileId is { } id ? $" (item {DescribeWorkshopItem(id)})" : string.Empty;
            throw new InvalidOperationException($"The Steam API call for {typeof(T).Name} failed before completing.{item}");
        }

        await RespondResultAsync(callbackId, result);
    }

    private static async Task<T?> WaitForCallAsync<T>(UInt64 call, int k_iCallback) where T : unmanaged
    {
        bool failed = false;
        while (!SteamUtils.IsAPICallCompleted(call, ref failed) && !failed)
            await Task.Delay(PollIntervalMs);

        // A reported failure is not a dead end: the caller can still inspect the live workshop
        // state to work out *why* (e.g. subscribe not yet applied vs. download rejected).
        return failed ? null : ReadCallResult<T>(call, k_iCallback);
    }

    private static T ReadCallResult<T>(UInt64 call, int k_iCallback) where T : unmanaged
    {
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];
        GCHandle pinned = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            IntPtr address = pinned.AddrOfPinnedObject();
            bool failed = false;
            if (!Steam.GetAPICallResult(Pipe, call, address, size, k_iCallback, ref failed) || failed)
                throw new InvalidOperationException($"Steam_GetAPICallResult for {typeof(T).Name} failed.");

            return Marshal.PtrToStructure<T>(address);
        }
        finally
        {
            pinned.Free();
        }
    }

    /// <summary>
    /// Captures the live Steam state of a workshop item for diagnostic messages after a failed
    /// call. <c>GetItemState</c>/<c>GetItemDownloadInfo</c>/<c>GetItemInstallInfo</c> are synchronous
    /// local queries, so they keep working even when the async call itself reports a failure.
    /// </summary>
    private static string DescribeWorkshopItem(ulong publishedFileId)
    {
        EItemState state = SteamUGC.GetItemState(publishedFileId);
        StringBuilder info = new($"state=[{DescribeState(state)}]");

        UInt64 bytesDownloaded = 0, bytesTotal = 0;
        if (SteamUGC.GetItemDownloadInfo(publishedFileId, ref bytesDownloaded, ref bytesTotal))
            info.Append($", download={bytesDownloaded}/{bytesTotal} bytes");

        UInt64 sizeOnDisk = 0;
        UInt32 timeStamp = 0;
        StringBuilder folder = new(1024);
        if (SteamUGC.GetItemInstallInfo(publishedFileId, ref sizeOnDisk, folder, (UInt32)folder.Capacity, ref timeStamp))
            info.Append($", onDisk={sizeOnDisk} bytes, folder=\"{folder}\"");

        return info.ToString();
    }

    private static string DescribeState(EItemState state)
    {
        if (state == EItemState.k_EItemStateNone)
            return "none";

        List<string> parts = new();
        if (state.HasFlag(EItemState.k_EItemStateSubscribed)) parts.Add("Subscribed");
        if (state.HasFlag(EItemState.k_EItemStateLegacyItem)) parts.Add("LegacyItem");
        if (state.HasFlag(EItemState.k_EItemStateInstalled)) parts.Add("Installed");
        if (state.HasFlag(EItemState.k_EItemStateNeedsUpdate)) parts.Add("NeedsUpdate");
        if (state.HasFlag(EItemState.k_EItemStateDownloading)) parts.Add("Downloading");
        if (state.HasFlag(EItemState.k_EItemStateDownloadPending)) parts.Add("DownloadPending");
        if (state.HasFlag(EItemState.k_EItemStateDisabledLocally)) parts.Add("DisabledLocally");

        return parts.Count > 0 ? string.Join("|", parts) : state.ToString();
    }

    private static Task RespondResultAsync<T>(string callbackId, T payload)
        => RespondAsync(callbackId, new { type = "result", payload });

    private static Task RespondProgressAsync<T>(string callbackId, T payload)
        => RespondAsync(callbackId, new { type = "progress", payload });

    private static Task RespondErrorAsync(string callbackId, string message)
        => RespondAsync(callbackId, new { type = "error", message });

    private static Task RespondErrorAsync(string callbackId, Exception ex)
        => RespondAsync(callbackId, new
        {
            type = "error",
            message = ex.Message,
            errorType = ex.GetType().FullName,
            stackTrace = ex.StackTrace,
        });

    private static async Task RespondAsync(string callbackId, object envelope)
    {
        string line = JsonSerializer.Serialize(envelope, JsonOptions);
        await OutputLock.WaitAsync();
        try
        {
            await Console.Out.WriteLineAsync($"{callbackId}\t{line}");
        }
        finally
        {
            OutputLock.Release();
        }
    }
}

