using Forge.Application.Common;
using Forge.Database;
using Forge.Database.Models;
using Forge.Realtime;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Features.Users.Commands
{
    public sealed record DeleteUserCommand(int UserId, int CurrentUserId) : IRequest<OperationResult<Unit>>;

    public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, OperationResult<Unit>>
    {
        private readonly ForgeDbContext _context;
        private readonly IRealtimeNotifier _notifier;

        public DeleteUserCommandHandler(ForgeDbContext context, IRealtimeNotifier notifier)
        {
            _context = context;
            _notifier = notifier;
        }

        public async Task<OperationResult<Unit>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId == request.CurrentUserId)
            {
                return OperationResult<Unit>.BadRequest("Nie można usunąć własnego konta.");
            }

            var user = await _context.Set<User>()
                .Include(u => u.Roles)
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return OperationResult<Unit>.NotFound();
            }

            if (user.Tasks.Count > 0)
            {
                return OperationResult<Unit>.Conflict("Nie można usunąć użytkownika, który ma przypisane zadania. Najpierw zarchiwizuj.");
            }

            _context.Set<User>().Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            await _notifier.EntityChangedAsync(RealtimeEntities.Users, cancellationToken: cancellationToken);

            return OperationResult<Unit>.Ok(Unit.Value);
        }
    }
}
