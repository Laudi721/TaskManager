using System.Collections.Concurrent;

namespace Forge.Realtime;

/// <summary>
/// Single-instance fallback used when Redis is not configured (dev mode without Redis).
/// Loses state on process restart and does NOT propagate across multiple API instances.
/// </summary>
internal sealed class InMemorySessionRegistry : ISessionRegistry
{
    private readonly ConcurrentDictionary<int, string> _sessions = new();

    public Task<string> StartAsync(int userId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        _sessions[userId] = sessionId;
        return Task.FromResult(sessionId);
    }

    public Task<bool> IsCurrentAsync(int userId, string sessionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sessions.TryGetValue(userId, out var stored) && stored == sessionId);
    }

    public Task EndAsync(int userId, CancellationToken cancellationToken = default)
    {
        _sessions.TryRemove(userId, out _);
        return Task.CompletedTask;
    }
}
