using Forge.Application.Common;
using Forge.Common.Dtos;
using Forge.Database;
using Forge.Database.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Features.UserSettings.Queries
{
    public sealed record GetMyPreferencesQuery(int UserId) : IRequest<OperationResult<UserPreferencesDto>>;

    public sealed class GetMyPreferencesQueryHandler : IRequestHandler<GetMyPreferencesQuery, OperationResult<UserPreferencesDto>>
    {
        private readonly ForgeDbContext _context;

        public GetMyPreferencesQueryHandler(ForgeDbContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<UserPreferencesDto>> Handle(GetMyPreferencesQuery request, CancellationToken cancellationToken)
        {
            var prefs = await _context.Set<User>()
                .AsNoTracking()
                .Where(u => u.Id == request.UserId)
                .Select(u => new UserPreferencesDto
                {
                    ThemePreference = u.ThemePreference
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (prefs == null)
            {
                return OperationResult<UserPreferencesDto>.NotFound();
            }

            return OperationResult<UserPreferencesDto>.Ok(prefs);
        }
    }
}
