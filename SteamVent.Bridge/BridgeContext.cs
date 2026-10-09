using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SteamVent.Core;
using SteamVent.Core.Steamworks;

namespace SteamVent.Bridge;

/// <summary>
/// Managed client for <c>SteamVentContextBridge.exe</c>.
/// <para>
/// The bridge is a child process that talks to the local Steam client on our behalf. Requests and
/// responses are single text lines correlated by a callback ID, so any number of commands may be in
/// flight simultaneously:
/// </para>
/// <code>
///   request : "&lt;callbackId&gt; &lt;command&gt; [args...]"
///   response: "&lt;callbackId&gt;\t{ "type": "result" | "progress" | "error", ... }"
/// </code>
/// <para>
/// <c>progress</c> lines are mid-work updates (routed to <see cref="BridgeOutput"/>); the final
/// <c>result</c>/<c>error</c> line for a callback ID completes the task the request is awaiting.
/// </para>
/// </summary>
public sealed class BridgeContext : IDisposable, IWorkshopSource, IWorkshopCommands
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(10);
    private const int MaxDiagnosticsLines = 200;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly UInt32 _appId;
    private readonly Process _process;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly object _pendingLock = new();
    private readonly Dictionary<string, PendingRequest> _pending = new();
    private readonly ConcurrentQueue<string> _recentOutput = new();
    private int _nextCallbackId;
    private bool _disposed;

    /// <summary>Raised with mid-work updates from the bridge (progress payloads, stderr output, ...).</summary>
    public event EventHandler<string>? BridgeOutput;

    /// <summary>The Steam app ID this bridge process is scoped to.</summary>
    public UInt32 AppId => _appId;

    /// <summary>True once <see cref="Dispose"/> has been called (used by the pool to skip dead entries).</summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Starts a new bridge process scoped to the given app.
    /// </summary>
    public BridgeContext(UInt32 appId)
    {
        _appId = appId;

        string? entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;
        if (entryAssemblyPath is null)
            throw new InvalidOperationException("Unable to resolve the entry assembly to locate SteamVentContextBridge.exe.");

        string directory = Path.GetDirectoryName(entryAssemblyPath)!;
        string bridgePath = Path.Combine(directory, "SteamVentContextBridge.exe");
        if (!File.Exists(bridgePath))
            throw new FileNotFoundException("SteamVentContextBridge.exe was not found next to the entry assembly.", bridgePath);

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = directory,
                FileName = bridgePath,
                Arguments = appId.ToString(CultureInfo.InvariantCulture),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        if (!_process.Start())
            throw new InvalidOperationException("Failed to start SteamVentContextBridge.exe.");

        _ = Task.Run(ReadLoopAsync);
        _ = Task.Run(StderrLoopAsync);
    }

    #region Workshop

    /// <summary>Subscribes to a workshop item. The call completes when Steam acknowledges the subscription.</summary>
    public Task<RemoteStorageSubscribePublishedFileResult_t> SteamWorkshopSubscribeAsync(UInt64 publishedFileId, CancellationToken cancellationToken = default)
        => SendAsync<RemoteStorageSubscribePublishedFileResult_t>($"workshop subscribe {publishedFileId}", DefaultTimeout, null, cancellationToken);

    /// <summary>
    /// Downloads (installs) a workshop item's content, completing only once it is fully installed. Emits
    /// <see cref="WorkshopItemProgress"/> samples on <paramref name="progress"/> (and on <see cref="BridgeOutput"/>)
    /// while the content is transferring.
    /// </summary>
    public Task<DownloadItemResult_t> SteamWorkshopDownloadAsync(UInt64 publishedFileId, IProgress<WorkshopItemProgress>? progress = null, CancellationToken cancellationToken = default)
        => SendAsync<DownloadItemResult_t>($"workshop download {publishedFileId}", DownloadTimeout, progress, cancellationToken);

    /// <summary>Unsubscribes from a workshop item, removing its installed content.</summary>
    public Task<RemoteStorageUnsubscribePublishedFileResult_t> SteamWorkshopUnsubscribeAsync(UInt64 publishedFileId, CancellationToken cancellationToken = default)
        => SendAsync<RemoteStorageUnsubscribePublishedFileResult_t>($"workshop unsubscribe {publishedFileId}", DefaultTimeout, null, cancellationToken);

    /// <summary>Lists all subscribed workshop items with their install/download state.</summary>
    public Task<PublishedFileData[]> SteamWorkshopListAsync(CancellationToken cancellationToken = default)
        => SendAsync<PublishedFileData[]>("workshop list", DefaultTimeout, null, cancellationToken);

    #endregion Workshop

    #region IWorkshopSource / IWorkshopCommands (shared surface with the in-process SteamVent.Steamworks.SteamWorkshop)

    /// <summary>
    /// The shared workshop command surface. <see cref="BridgeContext"/> is scoped to a single app (its
    /// <see cref="AppId"/>), so the <c>appid</c> argument is validated against that scope rather than used to pick a
    /// target - same operations, same behavior as the in-process backend.
    /// </summary>
    public Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
    {
        CheckAppId(appid);
        return SteamWorkshopSubscribeAsync(publishedFileId, cancellationToken);
    }

    public Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeAsync(ulong publishedFileId, int appid, CancellationToken cancellationToken = default)
    {
        CheckAppId(appid);
        return SteamWorkshopUnsubscribeAsync(publishedFileId, cancellationToken);
    }

    /// <summary>
    /// Downloads (installs) a workshop item and completes only once it is fully installed - the same contract as the
    /// in-process backend. Reports <paramref name="progress"/> samples while the content transfers.
    /// </summary>
    public Task<DownloadItemResult_t> DownloadAsync(ulong publishedFileId, int appid, IProgress<WorkshopItemProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        CheckAppId(appid);
        return SteamWorkshopDownloadAsync(publishedFileId, progress, cancellationToken);
    }

    public Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
    {
        CheckAppId(appid);
        return ToReadOnlyAsync(SteamWorkshopListAsync(cancellationToken));
    }

    public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
        => GetInstalledWorkshopItems(appid, cancellationToken);

    private static async Task<IReadOnlyCollection<PublishedFileData>> ToReadOnlyAsync(Task<PublishedFileData[]> items)
        => await items;

    private void CheckAppId(int appid)
    {
        if (appid != 0 && appid != (int)_appId)
            throw new WorkshopException($"This bridge is scoped to AppID {_appId}, but AppID {appid} was requested.", null)
            {
                AppId = (UInt32)appid,
                IsAppIdMismatch = true,
            };
    }

    #endregion IWorkshopSource / IWorkshopCommands

    #region Request/response plumbing

    private sealed class Envelope
    {
        public string? type { get; set; }
        public JsonElement? payload { get; set; }
        public string? message { get; set; }
        public string? errorType { get; set; }
        public string? stackTrace { get; set; }
    }

    private sealed class PendingRequest
    {
        public required TaskCompletionSource<string> Completion { get; init; }
        public string Command { get; init; } = string.Empty;
        public IProgress<WorkshopItemProgress>? Progress { get; init; }
        public ulong? PublishedFileId { get; init; }
    }

    private async Task<T> SendAsync<T>(string command, TimeSpan timeout, IProgress<WorkshopItemProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskCompletionSource<string> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        string callbackId = Interlocked.Increment(ref _nextCallbackId).ToString(CultureInfo.InvariantCulture);

        lock (_pendingLock)
            _pending[callbackId] = new PendingRequest
            {
                Completion = completion,
                Command = command,
                Progress = progress,
                PublishedFileId = TryParsePublishedFileId(command),
            };

        try
        {
            await WriteLineAsync($"{callbackId} {command}");
        }
        catch (Exception ex)
        {
            FailCallback(callbackId, "write failure", "Unable to send the request to the bridge.", ex);
            throw;
        }

        string payload;
        try
        {
            // Throws TimeoutException when the bridge never answers, OperationCanceledException when the caller cancels.
            payload = await completion.Task.WaitAsync(timeout, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Caller cancelled: abandon this in-flight request. The bridge child may keep working in the background;
            // it is isolated, and the pool reaps it once it (and any other commands) go idle.
            TryRemovePending(callbackId, out _);
            throw;
        }
        catch (TimeoutException)
        {
            TryRemovePending(callbackId, out _);
            throw CreateException(command, callbackId, "timeout",
                $"Timed out after {timeout} waiting for the bridge to complete.");
        }

        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions)!;
        }
        catch (JsonException ex)
        {
            throw CreateException(command, callbackId, "decode failure",
                "The bridge sent a response that could not be decoded.", ex, raw: payload);
        }
    }

    private static ulong? TryParsePublishedFileId(string command)
    {
        // e.g. "workshop download 12345" -> 12345
        int i = command.LastIndexOf(' ');
        if (i < 0 || i + 1 >= command.Length)
            return null;
        return ulong.TryParse(command[(i + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out ulong id) ? id : null;
    }

    private async Task WriteLineAsync(string line)
    {
        await _writeLock.WaitAsync();
        try
        {
            await _process.StandardInput.WriteLineAsync(line);
            await _process.StandardInput.FlushAsync();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task ReadLoopAsync()
    {
        try
        {
            while (await _process.StandardOutput.ReadLineAsync() is { } line)
            {
                int separator = line.IndexOf('\t');
                if (separator <= 0)
                {
                    OnBridgeOutput(line); // not protocol data; surface it verbatim
                    continue;
                }

                string callbackId = line[..separator];
                string json = line[(separator + 1)..];

                Envelope? envelope;
                try
                {
                    envelope = JsonSerializer.Deserialize<Envelope>(json, JsonOptions);
                }
                catch (JsonException ex)
                {
                    FailCallback(callbackId, "malformed response", "The bridge sent a malformed response.", ex, raw: json);
                    continue;
                }

                if (envelope is null)
                {
                    FailCallback(callbackId, "malformed response", "The bridge sent an empty response.");
                    continue;
                }

                switch (envelope.type)
                {
                    case "result":
                        if (envelope.payload is not null)
                            CompleteCallback(callbackId, envelope.payload.Value.GetRawText());
                        else
                            FailCallback(callbackId, "protocol error", "The bridge sent a result without a payload.", raw: json);
                        break;

                    case "error":
                    {
                        string detail = envelope.message ?? "The bridge reported an error.";
                        if (envelope.errorType is not null)
                            detail = $"{envelope.errorType}: {detail}";
                        FailCallback(callbackId, "bridge error", detail,
                            errorType: envelope.errorType, stackTrace: envelope.stackTrace, raw: json);
                        break;
                    }

                    case "progress":
                        DeliverProgress(callbackId, envelope.payload);
                        OnBridgeOutput(envelope.payload?.GetRawText() ?? string.Empty);
                        break;

                    default:
                        OnBridgeOutput(json);
                        break;
                }
            }
        }
        catch (Exception)
        {
            // The bridge dying is handled in the finally block below.
        }
        finally
        {
            int? exitCode = null;
            if (_process.HasExited)
            {
                try { exitCode = _process.ExitCode; }
                catch (InvalidOperationException) { /* process not fully reaped yet */ }
            }

            FailAllPending(exitCode);
        }
    }

    private async Task StderrLoopAsync()
    {
        try
        {
            while (await _process.StandardError.ReadLineAsync() is { } line)
                OnBridgeOutput($"[stderr] {line}");
        }
        catch (Exception)
        {
            // stderr is best-effort diagnostics only
        }
    }

    private void CompleteCallback(string callbackId, string payload)
    {
        if (TryRemovePending(callbackId, out PendingRequest? pending))
            pending!.Completion.TrySetResult(payload);
    }

    private void FailCallback(
        string callbackId,
        string reason,
        string detail,
        Exception? innerException = null,
        string? errorType = null,
        string? stackTrace = null,
        string? raw = null)
    {
        if (!TryRemovePending(callbackId, out PendingRequest? pending))
            return;

        pending!.Completion.TrySetException(CreateException(pending.Command, callbackId, reason, detail, innerException,
            errorType: errorType, stackTrace: stackTrace, raw: raw, publishedFileId: pending.PublishedFileId));
    }

    private bool TryRemovePending(string callbackId, out PendingRequest? pending)
    {
        lock (_pendingLock)
        {
            bool found = _pending.TryGetValue(callbackId, out pending);
            if (found)
                _pending.Remove(callbackId);
            return found;
        }
    }

    private void FailAllPending(int? exitCode)
    {
        List<KeyValuePair<string, PendingRequest>> abandoned;
        lock (_pendingLock)
        {
            abandoned = _pending.ToList();
            _pending.Clear();
        }

        string detail = exitCode == 0
            ? "The SteamVentContextBridge process exited."
            : $"The SteamVentContextBridge process exited unexpectedly (code {exitCode}).";

        foreach (KeyValuePair<string, PendingRequest> entry in abandoned)
            entry.Value.Completion.TrySetException(CreateException(entry.Value.Command, entry.Key, "process exit", detail,
                exitCode: exitCode, publishedFileId: entry.Value.PublishedFileId));
    }

    /// <summary>
    /// Builds a self-describing <see cref="BridgeException"/>: the message carries the command, app id, reason
    /// and last bridge output (so it is informative even in runners that only print the message), while the
    /// individual fields stay available for programmatic access and <see cref="BridgeException.ToString"/>.
    /// </summary>
    private WorkshopException CreateException(
        string command,
        string callbackId,
        string reason,
        string detail,
        Exception? innerException = null,
        int? exitCode = null,
        string? errorType = null,
        string? stackTrace = null,
        string? raw = null,
        ulong? publishedFileId = null)
    {
        // The BridgeException keeps all of the bridge-specific diagnostics (raw response, stderr tail, the bridge-side
        // exception type/stack trace). The WorkshopException is the unified type the caller catches; the bridge
        // failure is boxed in it as the InnerException so those diagnostics stay reachable. When the bridge reported
        // its own WorkshopException (the AppID-mismatch guard), that flag is surfaced on the unified exception too.
        BridgeException bridge = new(BuildFailureMessage(command, reason, detail), innerException)
        {
            Command = command,
            CallbackId = callbackId,
            AppId = _appId,
            Reason = reason,
            ExitCode = exitCode,
            BridgeExceptionType = errorType,
            BridgeStackTrace = stackTrace,
            RawResponse = raw,
            BridgeOutput = _recentOutput.ToArray(),
        };

        bool appIdMismatch = errorType is not null
            && (errorType == nameof(WorkshopException)
                || errorType.EndsWith("." + nameof(WorkshopException), StringComparison.Ordinal));

        return new WorkshopException(bridge.Message, bridge)
        {
            AppId = _appId,
            PublishedFileId = publishedFileId,
            Operation = command,
            IsAppIdMismatch = appIdMismatch,
        };
    }

    private string BuildFailureMessage(string command, string reason, string detail)
    {
        string message = $"[{command}, app {_appId}, {reason}] {detail}";
        string? tail = _recentOutput.Reverse().FirstOrDefault(l => l.Trim().Length > 0);
        if (tail is not null)
            message += $"  (last bridge output: {tail})";
        return message;
    }

    private void OnBridgeOutput(string message)
    {
        _recentOutput.Enqueue(message);
        while (_recentOutput.Count > MaxDiagnosticsLines)
            _recentOutput.TryDequeue(out _);
        BridgeOutput?.Invoke(this, message);
    }

    private bool TryGetPending(string callbackId, out PendingRequest? pending)
    {
        lock (_pendingLock)
            return _pending.TryGetValue(callbackId, out pending);
    }

    /// <summary>
    /// Routes a <c>progress</c> envelope to the awaiting request's <see cref="IProgress{T}"/> (when one was supplied
    /// for the download), so a caller sees live transfer status. Non-download commands have no progress sink, so this
    /// is a no-op for them.
    /// </summary>
    private void DeliverProgress(string callbackId, JsonElement? payload)
    {
        if (payload is not { } element)
            return;
        if (!TryGetPending(callbackId, out PendingRequest? pending) || pending!.Progress is not { } progress)
            return;

        progress.Report(new WorkshopItemProgress
        {
            PublishedFileId = pending.PublishedFileId ?? 0,
            State = ReadProgressState(element),
            BytesDownloaded = ReadProgressBytes(element, "bytesDownloaded"),
            BytesTotal = ReadProgressBytes(element, "bytesTotal"),
        });
    }

    private static EItemState ReadProgressState(JsonElement element)
    {
        if (element.TryGetProperty("state", out var state) && state.ValueKind == JsonValueKind.Number)
            return (EItemState)state.GetInt64();
        return EItemState.k_EItemStateNone;
    }

    private static ulong ReadProgressBytes(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetUInt64() : 0;

    #endregion Request/response plumbing

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;

        if (disposing)
        {
            try
            {
                _process.StandardInput.WriteLine("exit");
                _process.StandardInput.Flush();
            }
            catch
            {
                // The process may already be gone; that's fine.
            }

            try
            {
                if (!_process.WaitForExit(5000))
                    _process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Already gone.
            }

            _process.Dispose();
            _writeLock.Dispose();
        }
    }

    ~BridgeContext()
    {
        Dispose(false);
    }

    #endregion IDisposable
}

