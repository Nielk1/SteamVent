namespace SteamVent.Bridge;

/// <summary>
/// Thrown when a request to <see cref="BridgeContext"/> fails: the bridge reported an error, the
/// response timed out, or the <c>SteamVentContextBridge</c> process died.
/// <para>
/// Beyond the message, the exception carries the context needed to diagnose the failure without the
/// logs: what command was in flight (for which app), how the failure was detected, the raw bridge
/// response, the last bridge diagnostics (stderr / progress), and — when the failure originated in the
/// bridge process — that exception's type and stack trace. All of this is included in
/// <see cref="ToString"/>.
/// </para>
/// </summary>
public class BridgeException : Exception
{
    /// <summary>The command that was in flight when the failure was detected (e.g. <c>"workshop download 123"</c>).</summary>
    public string? Command { get; init; }

    /// <summary>The callback ID that correlated the failing request, if known.</summary>
    public string? CallbackId { get; init; }

    /// <summary>The Steam app ID the bridge was scoped to, if known.</summary>
    public UInt32? AppId { get; init; }

    /// <summary>
    /// How the failure was detected: <c>bridge error</c>, <c>timeout</c>, <c>process exit</c>,
    /// <c>malformed response</c>, or <c>decode failure</c>.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>The bridge process exit code, when the failure was caused by the process ending.</summary>
    public int? ExitCode { get; init; }

    /// <summary>The raw response envelope/payload received from the bridge, when one was received.</summary>
    public string? RawResponse { get; init; }

    /// <summary>The exception type reported by the bridge, when the failure originated in the bridge process.</summary>
    public string? BridgeExceptionType { get; init; }

    /// <summary>The stack trace reported by the bridge, when the failure originated in the bridge process.</summary>
    public string? BridgeStackTrace { get; init; }

    /// <summary>The most recent bridge diagnostics (stderr, progress updates, non-protocol stdout).</summary>
    public IReadOnlyList<string>? BridgeOutput { get; init; }

    public BridgeException(string message)
        : base(message)
    {
    }

    public BridgeException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public override string ToString()
    {
        // The message already carries the command, app id, reason, and last bridge output, so only append
        // the deeper context (bridge-side exception, raw response, and the full diagnostics tail).
        string baseText = base.ToString()!;

        if (BridgeExceptionType is not null)
            baseText += Environment.NewLine + $"Bridge exception type: {BridgeExceptionType}";

        if (BridgeStackTrace is not null)
            baseText += Environment.NewLine + "Bridge stack trace:" + Environment.NewLine + BridgeStackTrace;

        if (RawResponse is not null)
            baseText += Environment.NewLine + Environment.NewLine + $"Raw response: {RawResponse}";

        if (BridgeOutput is { Count: > 0 })
        {
            baseText += Environment.NewLine + Environment.NewLine + "Bridge output:";
            foreach (string line in BridgeOutput)
                baseText += Environment.NewLine + "  " + line;
        }

        return baseText;
    }
}
