namespace Forge.Realtime;

public interface IRealtimeNotifier
{
    Task EntityChangedAsync(string entity, object? payload = null, CancellationToken cancellationToken = default);
}
