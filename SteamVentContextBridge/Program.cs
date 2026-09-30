using SteamVent.Bridge;
using SteamVent.Common.InterProc;
using SteamVent.InterProc;
using SteamVent.InterProc.Interfaces;
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

    private static async Task<int> Main(string[] args)
    {
        if (args.Length != 1 || !UInt32.TryParse(args[0], out UInt32 appId) || appId == 0)
        {
            Console.Error.WriteLine("Usage: SteamVentContextBridge <AppId>");
            return 1;
        }

        // Scope the workshop calls to the requested app without needing a steam_appid.txt.
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
            await RespondErrorAsync(callbackId, ex.Message);
        }
    }

    private static async Task WorkshopAsync(string callbackId, string args)
    {
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
                            callbackId, SteamUGC.SubscribeItem(publishedFileId), RemoteStorageSubscribePublishedFileResult_t.k_iCallback);
                        break;

                    case "unsubscribe":
                        await RunCallAsync<RemoteStorageUnsubscribePublishedFileResult_t>(
                            callbackId, SteamUGC.UnsubscribeItem(publishedFileId), RemoteStorageUnsubscribePublishedFileResult_t.k_iCallback);
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

        DownloadItemResult_t result = await WaitForCallAsync<DownloadItemResult_t>(
            SteamUGC.DownloadItem(publishedFileId), DownloadItemResult_t.k_iCallback);

        if (result.m_eResult != EResult.k_EResultOK)
            throw new InvalidOperationException($"DownloadItem({publishedFileId}) returned {result.m_eResult}.");

        DateTime deadline = DateTime.UtcNow + DownloadDeadline;
        while (true)
        {
            EItemState state = SteamUGC.GetItemState(publishedFileId);

            if (IsInstalledAndCurrent(state))
            {
                await RespondResultAsync(callbackId, result);
                return;
            }

            bool active = state.HasFlag(EItemState.k_EItemStateDownloading)
                || state.HasFlag(EItemState.k_EItemStateDownloadPending);
            if (!active)
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

    private static async Task RunCallAsync<T>(string callbackId, UInt64 call, int k_iCallback) where T : unmanaged
    {
        T result = await WaitForCallAsync<T>(call, k_iCallback);
        await RespondResultAsync(callbackId, result);
    }

    private static async Task<T> WaitForCallAsync<T>(UInt64 call, int k_iCallback) where T : unmanaged
    {
        bool failed = false;
        while (!SteamUtils.IsAPICallCompleted(call, ref failed) && !failed)
            await Task.Delay(PollIntervalMs);

        if (failed)
            throw new InvalidOperationException("The Steam API call failed before completing.");

        return ReadCallResult<T>(call, k_iCallback);
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

    private static Task RespondResultAsync<T>(string callbackId, T payload)
        => RespondAsync(callbackId, new { type = "result", payload });

    private static Task RespondProgressAsync<T>(string callbackId, T payload)
        => RespondAsync(callbackId, new { type = "progress", payload });

    private static Task RespondErrorAsync(string callbackId, string message)
        => RespondAsync(callbackId, new { type = "error", message });

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

