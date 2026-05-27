using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Forge.Common.Interfaces;
using Forge.Database;
using Forge.Database.Models;
using Forge.Desk.WebApi.Security;
using Microsoft.EntityFrameworkCore;
using Forge.Common.Dtos;
using Forge.Realtime;

namespace Forge.Desk.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ForgeDbContext _db;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly ISessionRegistry _sessions;
        private readonly IHubContext<NotificationsHub> _hub;

        public AuthController(
            ForgeDbContext db,
            IPasswordService passwordService,
            ITokenService tokenService,
            ISessionRegistry sessions,
            IHubContext<NotificationsHub> hub)
        {
            _db = db;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _sessions = sessions;
            _hub = hub;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse { Success = false, Message = "Login and password are required." });
            }

            var user = await _db.Set<User>()
                .FirstOrDefaultAsync(u => u.Login == request.Login && !u.IsArchive, cancellationToken);

            if (user == null || !_passwordService.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new LoginResponse { Success = false, Message = "Invalid login or password." });
            }

            // Overwriting any previous session for this user: from this point any API call carrying
            // the previous JWT will fail validation (sid claim no longer matches the registry).
            // Use a small buffer over token TTL so the registry survives clock skew during validation.
            var sessionTtl = _tokenService.TokenLifetime + TimeSpan.FromMinutes(5);
            var sessionId = await _sessions.StartAsync(user.Id, sessionTtl, cancellationToken);

            // Notify any still-open client of the previous session to log out immediately.
            // Delivered via SignalR Clients.User(userId) thanks to NameIdentifierUserIdProvider.
            await _hub.Clients.User(user.Id.ToString())
                .SendAsync(RealtimeEvents.SessionRevoked, new { reason = "taken-over" }, cancellationToken);

            var token = _tokenService.CreateToken(user, sessionId);

            return Ok(new LoginResponse
            {
                Success = true,
                UserId = user.Id,
                Name = user.Name,
                Login = user.Login,
                Token = token.Token,
                ExpiresAtUtc = token.ExpiresAtUtc,
                Preferences = new UserPreferencesDto
                {
                    ThemePreference = user.ThemePreference
                }
            });
        }
    }
}
