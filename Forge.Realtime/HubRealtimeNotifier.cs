using Microsoft.AspNetCore.SignalR;

namespace Forge.Realtime;

internal sealed class HubRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationsHub> _hub;

    public HubRealtimeNotifier(IHubContext<NotificationsHub> hub)
    {
        _hub = hub;
    }

    public Task EntityChangedAsync(string entity, object? payload = null, CancellationToken cancellationToken = default)
    {
        var message = new EntityChangedMessage(entity, payload);
        return _hub.Clients.All.SendAsync(RealtimeEvents.EntityChanged, message, cancellationToken);
    }

    private sealed record EntityChangedMessage(string Entity, object? Payload);
}
