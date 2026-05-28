using Forge.Application.Common;
using Forge.Database;
using Forge.Database.Models;
using Forge.Realtime;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Features.Users.Commands
{
    public sealed record ToggleUserArchiveCommand(int UserId, int CurrentUserId) : IRequest<OperationResult<Unit>>;

    public sealed class ToggleUserArchiveCommandHandler : IRequestHandler<ToggleUserArchiveCommand, OperationResult<Unit>>
    {
        private readonly ForgeDbContext _context;
        private readonly IRealtimeNotifier _notifier;

        public ToggleUserArchiveCommandHandler(ForgeDbContext context, IRealtimeNotifier notifier)
        {
            _context = context;
            _notifier = notifier;
        }

        public async Task<OperationResult<Unit>> Handle(ToggleUserArchiveCommand request, CancellationToken cancellationToken)
        {
            var role = await _context.Set<Role>()
                .Include(a => a.Users)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            
            if (role.Users.Any())
            {
                return OperationResult<Unit>.BadRequest("Nie można zarchiwizować przypisanej roli");
            }

            role.IsArchive = !role.IsArchive;

            await _context.SaveChangesAsync(cancellationToken);

            await _notifier.EntityChangedAsync(RealtimeEntities.Users, cancellationToken: cancellationToken);

            return OperationResult<Unit>.Ok(Unit.Value);
        }
    }
}
