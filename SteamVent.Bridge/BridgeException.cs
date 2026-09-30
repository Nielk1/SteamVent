namespace SteamVent.Bridge;

/// <summary>
/// Thrown when a request to <see cref="BridgeContext"/> fails: the bridge reported an error, the
/// response timed out, or the <c>SteamVentContextBridge</c> process died.
/// </summary>
public class BridgeException : Exception
{
    public BridgeException(string message)
        : base(message)
    {
    }

    public BridgeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
