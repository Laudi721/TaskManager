using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Forge.Realtime;

/// <summary>
/// Maps SignalR's Clients.User(id) to the JWT NameIdentifier (user.Id) instead of the default
/// ClaimTypes.Name (login). This lets us target a specific user by integer Id.
/// </summary>
public sealed class NameIdentifierUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
