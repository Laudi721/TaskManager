namespace Forge.Realtime;

/// <summary>
/// Tracks the currently-active session per user. Used to enforce single-session-per-user:
/// when a user logs in, a new sessionId is stored and the previous one is invalidated.
/// JWT validation checks that the token's 'sid' claim matches the registered sessionId.
/// </summary>
public interface ISessionRegistry
{
    /// <summary>
    /// Issues a new sessionId for the given user and stores it as the currently-active one,
    /// overwriting any previous session. TTL defaults to <paramref name="ttl"/>.
    /// </summary>
    Task<string> StartAsync(int userId, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true when <paramref name="sessionId"/> equals the currently-stored sessionId for the user.
    /// </summary>
    Task<bool> IsCurrentAsync(int userId, string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the session entry (used at logout). Subsequent API calls with the old token will fail.
    /// </summary>
    Task EndAsync(int userId, CancellationToken cancellationToken = default);
}
