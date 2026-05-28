namespace Forge.Realtime;

public interface IRealtimeNotifier
{
    Task EntityChangedAsync(string entity, object? payload = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify a specific user that their session has been revoked (e.g. taken over by a new login).
    /// </summary>
    Task SessionRevokedAsync(int userId, string reason, CancellationToken cancellationToken = default);
}
