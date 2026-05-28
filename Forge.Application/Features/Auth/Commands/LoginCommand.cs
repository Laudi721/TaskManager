using Forge.Application.Common;
using Forge.Application.Features.Auth.Requests;
using Forge.Common.Dtos;
using Forge.Common.Interfaces;
using Forge.Database;
using Forge.Database.Models;
using Forge.Realtime;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Features.Auth.Commands
{
    public sealed record LoginCommand(LoginRequest Request) : IRequest<OperationResult<LoginResponse>>;

    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, OperationResult<LoginResponse>>
    {
        private readonly ForgeDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly ISessionRegistry _sessions;
        private readonly IRealtimeNotifier _notifier;

        public LoginCommandHandler(
            ForgeDbContext context,
            IPasswordService passwordService,
            ITokenService tokenService,
            ISessionRegistry sessions,
            IRealtimeNotifier notifier)
        {
            _context = context;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _sessions = sessions;
            _notifier = notifier;
        }

        public async Task<OperationResult<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(request.Login))
            {
                errors.Add(new ValidationError(nameof(request.Login), "Login jest wymagany."));
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                errors.Add(new ValidationError(nameof(request.Password), "Hasło jest wymagane."));
            }

            if (errors.Count > 0)
            {
                return OperationResult<LoginResponse>.ValidationFailed(errors);
            }

            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Login == request.Login && !u.IsArchive, cancellationToken);

            if (user == null || !_passwordService.Verify(request.Password, user.PasswordHash))
            {
                return OperationResult<LoginResponse>.Unauthorized("Nieprawidłowy login lub hasło.");
            }

            // Overwriting any previous session for this user: from this point any API call carrying
            // the previous JWT will fail validation (sid claim no longer matches the registry).
            // Buffer over token TTL keeps the registry alive through clock skew during validation.
            var sessionTtl = _tokenService.TokenLifetime + TimeSpan.FromMinutes(5);
            var sessionId = await _sessions.StartAsync(user.Id, sessionTtl, cancellationToken);

            await _notifier.SessionRevokedAsync(user.Id, "taken-over", cancellationToken);

            var token = _tokenService.CreateToken(user, sessionId);

            var response = new LoginResponse
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
            };

            return OperationResult<LoginResponse>.Ok(response);
        }
    }
}
