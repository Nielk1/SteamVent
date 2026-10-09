using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteamVent.Bridge;

/// <summary>
/// A process pool of <see cref="BridgeContext"/> instances, keyed by Steam AppID. An entry is created on first use
/// for an appid and kept warm for subsequent work on that appid (so a burst of commands reuses one child process
/// instead of spawning one per command), then reaped once it has been idle for <see cref="IdleTimeout"/>. Different
/// appids run concurrently, each with its own child process, and each decays on its own schedule - so asking for
/// work on two appids at once yields two live bridges, and each falls off independently when it stops being asked.
/// <para>
/// Keep <see cref="IdleTimeout"/> short: an idle bridge still holds a Steam app-context alive (it can make Steam
/// treat that app as running), so a long hang time is actively harmful.
/// </para>
/// </summary>
public sealed class BridgePool : IDisposable
{
    private sealed class Entry
    {
        public required BridgeContext Context { get; init; }
        public int Active;
        public DateTime LastActive;
    }

    private readonly object _gate = new();
    private readonly Dictionary<UInt32, Entry> _entries = new();
    private readonly Timer _reaper;
    private bool _disposed;

    /// <summary>How long an idle entry stays warm before it is reaped.</summary>
    public TimeSpan IdleTimeout { get; set; }

    /// <summary>How often the reaper looks for entries that have gone idle past <see cref="IdleTimeout"/>.</summary>
    public TimeSpan ReapInterval { get; set; }

    /// <summary>Creates a pool with the given idle timeout (defaults to 30s when null or non-positive).</summary>
    public BridgePool(TimeSpan? idleTimeout = null)
    {
        IdleTimeout = idleTimeout is { TotalSeconds: > 0 } value ? value : TimeSpan.FromSeconds(30);
        ReapInterval = TimeSpan.FromSeconds(2);
        _reaper = new Timer(OnReap, null, ReapInterval, ReapInterval);
    }

    /// <summary>The shared pool used by the adaptive layer and, by default, direct users.</summary>
    public static BridgePool Default { get; } = new();

    /// <summary>
    /// Gets (or creates) the warm <see cref="BridgeContext"/> for <paramref name="appId"/> and reserves it for the
    /// caller's unit of work; pair with <see cref="Release(UInt32)"/> in a <c>finally</c>, or use
    /// <see cref="WithAsync{T}"/> / <see cref="With{T}"/> to handle both automatically.
    /// </summary>
    public BridgeContext Acquire(UInt32 appId)
    {
        ThrowIfDisposed();
        lock (_gate)
        {
            if (_entries.TryGetValue(appId, out Entry? existing) && !existing.Context.IsDisposed)
            {
                existing.Active++;
                existing.LastActive = DateTime.UtcNow;
                return existing.Context;
            }

            Entry entry = new() { Context = new BridgeContext(appId) };
            entry.Active = 1;
            entry.LastActive = DateTime.UtcNow;
            _entries[appId] = entry;
            return entry.Context;
        }
    }

    /// <summary>Releases the reservation taken by <see cref="Acquire(UInt32)"/> and refreshes the idle clock.</summary>
    public void Release(UInt32 appId)
    {
        lock (_gate)
        {
            if (_entries.TryGetValue(appId, out Entry? entry) && entry.Active > 0)
            {
                entry.Active--;
                entry.LastActive = DateTime.UtcNow;
            }
        }
    }

    /// <summary>Runs an async unit of work against the warm context for <paramref name="appId"/>, handling acquire/release.</summary>
    public async Task<T> WithAsync<T>(UInt32 appId, Func<BridgeContext, Task<T>> operation)
    {
        BridgeContext context = Acquire(appId);
        try
        {
            return await operation(context);
        }
        finally
        {
            Release(appId);
        }
    }

    /// <summary>Runs a synchronous unit of work against the warm context for <paramref name="appId"/>, handling acquire/release.</summary>
    public T With<T>(UInt32 appId, Func<BridgeContext, T> operation)
    {
        BridgeContext context = Acquire(appId);
        try
        {
            return operation(context);
        }
        finally
        {
            Release(appId);
        }
    }

    /// <summary>The appids currently held warm in this pool (for diagnostics).</summary>
    public IReadOnlyCollection<UInt32> AppIds
    {
        get { lock (_gate) return _entries.Keys.ToArray(); }
    }

    private void OnReap(object? state)
    {
        List<BridgeContext> toDispose;
        lock (_gate)
        {
            if (_disposed)
                return;

            DateTime now = DateTime.UtcNow;
            List<UInt32> expired = new();
            foreach (KeyValuePair<UInt32, Entry> pair in _entries)
                if (pair.Value.Active == 0 && now - pair.Value.LastActive >= IdleTimeout)
                    expired.Add(pair.Key);

            toDispose = new List<BridgeContext>(expired.Count);
            foreach (UInt32 appId in expired)
            {
                toDispose.Add(_entries[appId].Context);
                _entries.Remove(appId);
            }
        }

        foreach (BridgeContext context in toDispose)
        {
            try { context.Dispose(); }
            catch { /* best effort: the child may already be gone */ }
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BridgePool));
    }

    public void Dispose()
    {
        List<BridgeContext> toDispose;
        lock (_gate)
        {
            if (_disposed)
                return;
            _disposed = true;
            toDispose = new List<BridgeContext>(_entries.Values.Select(e => e.Context));
            _entries.Clear();
        }

        _reaper.Dispose();
        foreach (BridgeContext context in toDispose)
        {
            try { context.Dispose(); }
            catch { }
        }
    }
}