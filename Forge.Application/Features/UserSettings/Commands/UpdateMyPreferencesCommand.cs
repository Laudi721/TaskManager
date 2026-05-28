using Forge.Application.Common;
using Forge.Application.Features.UserSettings.Requests;
using Forge.Database;
using Forge.Database.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Features.UserSettings.Commands
{
    public sealed record UpdateMyPreferencesCommand(int UserId, UserPreferencesUpdateRequest Request)
        : IRequest<OperationResult<Unit>>;

    public sealed class UpdateMyPreferencesCommandHandler
        : IRequestHandler<UpdateMyPreferencesCommand, OperationResult<Unit>>
    {
        private const int MaxThemeLength = 64;

        private readonly ForgeDbContext _context;

        public UpdateMyPreferencesCommandHandler(ForgeDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<Unit>> Handle(UpdateMyPreferencesCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return OperationResult<Unit>.NotFound();
            }

            user.ThemePreference = SanitizeTheme(request.Request.ThemePreference);

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult<Unit>.Ok(Unit.Value);
        }

        private static string? SanitizeTheme(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var trimmed = raw.Trim();
            return trimmed.Length > MaxThemeLength ? trimmed[..MaxThemeLength] : trimmed;
        }
    }
}
