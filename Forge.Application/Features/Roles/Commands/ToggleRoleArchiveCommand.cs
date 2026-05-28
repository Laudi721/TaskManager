using System;
using System.Collections.Generic;
using System.Text;
using Forge.Application.Common;
using Forge.Database;
using Forge.Database.Models;
using Forge.Realtime;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Features.Roles.Commands
{
    public sealed record ToggleRoleArchiveCommand(int id) : IRequest<OperationResult<Unit>>;

    public sealed class ToggleRoleArchiveCommandHandler : IRequestHandler<ToggleRoleArchiveCommand, OperationResult<Unit>>
    {
        private readonly ForgeDbContext _context;
        private readonly IRealtimeNotifier _notifier;

        public ToggleRoleArchiveCommandHandler(ForgeDbContext context,
            IRealtimeNotifier notifier)
        {
            _context = context;
            _notifier = notifier;
        }

        public async Task<OperationResult<Unit>> Handle(ToggleRoleArchiveCommand request, CancellationToken cancellationToken)
        {
            var role = await _context.Set<Role>()
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Id == request.id);

            if (role is null)
                return OperationResult<Unit>.BadRequest("Nie znaleziono roli o podanym ID");
            if (role.Users.Any())
                return OperationResult<Unit>.BadRequest("Nie można zarchiwizować roli, która jest przypisana do użytkowników");

            role.IsArchive = !role.IsArchive;

            await _context.SaveChangesAsync();
            await _notifier.EntityChangedAsync(RealtimeEntities.Roles, cancellationToken);

            return OperationResult<Unit>.Ok(Unit.Value, "Rola została zarchiwizowana");
        }
    }
}
