using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Forge.Realtime;

[Authorize]
public class NotificationsHub : Hub
{
}
