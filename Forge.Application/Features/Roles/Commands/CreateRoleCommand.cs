using Forge.Application.Common;
using Forge.Application.Features.Roles.Requests;
using Forge.Common.Dtos;
using Forge.Database;
using Forge.Database.Models;
using Forge.Realtime;
using MediatR;

namespace Forge.Application.Features.Roles.Commands
{
    public sealed record CreateRoleCommand(RoleCreateRequest Dto) : IRequest<OperationResult<Unit>>;

    public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, OperationResult<Unit>>
    {
        private readonly ForgeDbContext _context;
        private readonly IRealtimeNotifier _notifier;

        public CreateRoleCommandHandler(ForgeDbContext context,
            IRealtimeNotifier notifier)
        {
            _context = context;
            _notifier = notifier;
        }

        public async Task<OperationResult<Unit>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var errors = new List<ValidationError>();

            var dto = request.Dto;

            var reserved = _context.Set<Role>().Any(a => a.Name == dto.Name);

            if (reserved)
                errors.Add(new ValidationError(nameof(dto.Name), "Nazwa roli nie moze sie powtarzać"));

            if(errors.Any())
                return OperationResult<Unit>.ValidationFailed(errors);

            var role = new Role
            {
                Name = dto.Name,
            };

            _context.Set<Role>().Add(role);
            await _context.SaveChangesAsync();

            await _notifier.EntityChangedAsync(RealtimeEntities.Roles, cancellationToken);

            return OperationResult<Unit>.Ok(Unit.Value, "Rola została utworzona pomyślnie");
        }
    }
}
