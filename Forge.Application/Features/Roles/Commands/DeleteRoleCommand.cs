using System;
using System.Collections.Generic;
using System.Text;
using Forge.Application.Common;
using Forge.Database;
using Forge.Database.Models;
using Forge.Realtime;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Forge.Application.Features.Roles.Commands
{
    public sealed record DeleteRoleCommand(int roleId) : IRequest<OperationResult<Unit>>;

    public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, OperationResult<Unit>>
    {
        private readonly ForgeDbContext _context;
        private readonly IRealtimeNotifier _notifier;

        public DeleteRoleCommandHandler(ForgeDbContext context,
            IRealtimeNotifier notifier)
        {
            _context = context;
            _notifier = notifier;
        }

        public async Task<OperationResult<Unit>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _context.Set<Role>()
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Id == request.roleId);

            if (role is null)
                return OperationResult<Unit>.BadRequest("Nie znaleziono roli o podanym ID");

            if(role.Users.Any())
                return OperationResult<Unit>.BadRequest("Nie można usunąć roli, która jest przypisana do użytkowników");

            _context.Remove(role);
            await _context.SaveChangesAsync();

            await _notifier.EntityChangedAsync(RealtimeEntities.Roles, cancellationToken);

            return OperationResult<Unit>.Ok(Unit.Value, "Rola została usunięta");
        }
    }
}
