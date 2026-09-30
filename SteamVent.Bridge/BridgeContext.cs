using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SteamVent.Common.InterProc;

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

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly Process _process;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly object _pendingLock = new();
    private readonly Dictionary<string, TaskCompletionSource<string>> _pending = new();
    private int _nextCallbackId;
    private bool _disposed;

    /// <summary>Raised with mid-work updates from the bridge (progress payloads, stderr output, ...).</summary>
    public event EventHandler<string>? BridgeOutput;

    /// <summary>
    /// Starts a new bridge process scoped to the given app.
    /// </summary>
    public BridgeContext(UInt32 appId)
    {
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
    }

    private async Task<T> SendAsync<T>(string command, TimeSpan timeout)
    {
        TaskCompletionSource<string> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        string callbackId = Interlocked.Increment(ref _nextCallbackId).ToString(CultureInfo.InvariantCulture);

        lock (_pendingLock)
            _pending[callbackId] = completion;

        try
        {
            await WriteLineAsync($"{callbackId} {command}");
        }
        catch (Exception ex)
        {
            FailCallback(callbackId, ex);
            throw;
        }

        string payload = await completion.Task.WaitAsync(timeout); // throws TimeoutException when the bridge never answers

        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new BridgeException($"Unable to decode the response to '{command}'.", ex);
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
                    FailCallback(callbackId, new BridgeException("The bridge sent a malformed response.", ex));
                    continue;
                }

                if (envelope is null)
                {
                    FailCallback(callbackId, new BridgeException("The bridge sent an empty response."));
                    continue;
                }

                switch (envelope.type)
                {
                    case "result":
                        if (envelope.payload is not null)
                            CompleteCallback(callbackId, envelope.payload.Value.GetRawText());
                        else
                            FailCallback(callbackId, new BridgeException("The bridge sent a result without a payload."));
                        break;

                    case "error":
                        FailCallback(callbackId, new BridgeException(envelope.message ?? "The bridge reported an error."));
                        break;

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
            FailAllPending(new BridgeException(
                _process.HasExited && _process.ExitCode == 0
                    ? "The SteamVentContextBridge process exited."
                    : $"The SteamVentContextBridge process exited unexpectedly (code {_process.ExitCode})."));
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
        if (TryRemovePending(callbackId, out TaskCompletionSource<string>? completion))
            completion!.TrySetResult(payload);
    }

    private void FailCallback(string callbackId, Exception exception)
    {
        if (TryRemovePending(callbackId, out TaskCompletionSource<string>? completion))
            completion!.TrySetException(exception);
    }

    private bool TryRemovePending(string callbackId, out TaskCompletionSource<string>? completion)
    {
        lock (_pendingLock)
        {
            bool found = _pending.TryGetValue(callbackId, out completion);
            if (found)
                _pending.Remove(callbackId);
            return found;
        }
    }

    private void FailAllPending(Exception exception)
    {
        List<TaskCompletionSource<string>> abandoned;
        lock (_pendingLock)
        {
            abandoned = _pending.Values.ToList();
            _pending.Clear();
        }

        foreach (TaskCompletionSource<string> completion in abandoned)
            completion.TrySetException(exception);
    }

    private void OnBridgeOutput(string message) => BridgeOutput?.Invoke(this, message);

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

