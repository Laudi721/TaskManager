using Forge.Common.Dtos;
using Forge.Database;
using Forge.Database.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Features.Users.Queries
{
    public sealed record GetUsersQuery() : IRequest<IReadOnlyList<UserDto>>;

    public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
    {
        private readonly ForgeDbContext _context;

        public GetUsersQueryHandler(ForgeDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken) => await _context.Set<User>()
            .AsNoTracking()
            .OrderBy(u => u.Login)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Login = u.Login,
                Name = u.Name,
                IsArchive = u.IsArchive,
                Roles = u.Roles.Select(r => r.Name).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
