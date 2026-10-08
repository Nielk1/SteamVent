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
public sealed class BridgeContext : IDisposable
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
    public Task<RemoteStorageSubscribePublishedFileResult_t> SteamWorkshopSubscribeAsync(UInt64 publishedFileId)
        => SendAsync<RemoteStorageSubscribePublishedFileResult_t>($"workshop subscribe {publishedFileId}", DefaultTimeout);

    /// <summary>
    /// Downloads (installs) a workshop item's content. Emits <c>progress</c> updates on
    /// <see cref="BridgeOutput"/> while the content is transferring; the task completes when the
    /// item is fully installed.
    /// </summary>
    public Task<DownloadItemResult_t> SteamWorkshopDownloadAsync(UInt64 publishedFileId)
        => SendAsync<DownloadItemResult_t>($"workshop download {publishedFileId}", DownloadTimeout);

    /// <summary>Unsubscribes from a workshop item, removing its installed content.</summary>
    public Task<RemoteStorageUnsubscribePublishedFileResult_t> SteamWorkshopUnsubscribeAsync(UInt64 publishedFileId)
        => SendAsync<RemoteStorageUnsubscribePublishedFileResult_t>($"workshop unsubscribe {publishedFileId}", DefaultTimeout);

    /// <summary>Lists all subscribed workshop items with their install/download state.</summary>
    public Task<PublishedFileData[]> SteamWorkshopListAsync()
        => SendAsync<PublishedFileData[]>("workshop list", DefaultTimeout);

    #endregion Workshop

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
    }

    private async Task<T> SendAsync<T>(string command, TimeSpan timeout)
    {
        TaskCompletionSource<string> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        string callbackId = Interlocked.Increment(ref _nextCallbackId).ToString(CultureInfo.InvariantCulture);

        lock (_pendingLock)
            _pending[callbackId] = new PendingRequest { Completion = completion, Command = command };

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
            payload = await completion.Task.WaitAsync(timeout); // throws TimeoutException when the bridge never answers
        }
        catch (TimeoutException)
        {
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
            errorType: errorType, stackTrace: stackTrace, raw: raw));
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
                exitCode: exitCode));
    }

    /// <summary>
    /// Builds a self-describing <see cref="BridgeException"/>: the message carries the command, app id, reason
    /// and last bridge output (so it is informative even in runners that only print the message), while the
    /// individual fields stay available for programmatic access and <see cref="BridgeException.ToString"/>.
    /// </summary>
    private BridgeException CreateException(
        string command,
        string callbackId,
        string reason,
        string detail,
        Exception? innerException = null,
        int? exitCode = null,
        string? errorType = null,
        string? stackTrace = null,
        string? raw = null)
    {
        return new BridgeException(BuildFailureMessage(command, reason, detail), innerException)
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

