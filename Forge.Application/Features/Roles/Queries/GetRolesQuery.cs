using Forge.Common.Dtos;
using Forge.Database;
using Forge.Database.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Features.Roles.Queries
{
    public sealed record GetRolesQuery() : IRequest<IReadOnlyList<RoleDto>>;
    public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
    {
        private readonly ForgeDbContext _context;

        public GetRolesQueryHandler(ForgeDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken) => await _context.Set<Role>()
            .AsNoTracking()
            .Select(a => new RoleDto
            {
                Id = a.Id,
                Name = a.Name,
                UsersCount = a.Users.Count,
                IsArchive = a.IsArchive
            }).ToListAsync(cancellationToken);        
    }
}
