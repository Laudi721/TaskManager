using Forge.Application.Common;
using Forge.Application.Features.Users.Requests;
using Forge.Common.Interfaces;
using Forge.Database;
using Forge.Database.Models;
using Forge.Realtime;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Features.Users.Commands
{
    public sealed record CreateUserCommand(UserCreateRequest Dto) : IRequest<OperationResult<Unit>>;

    public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, OperationResult<Unit>>
    {
        private readonly ForgeDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IRealtimeNotifier _notifier;

        public CreateUserCommandHandler(
            ForgeDbContext context,
            IPasswordService passwordService,
            IRealtimeNotifier notifier)
        {
            _context = context;
            _passwordService = passwordService;
            _notifier = notifier;
        }

        public async Task<OperationResult<Unit>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(dto.Login))
            {
                errors.Add(new ValidationError(nameof(dto.Login), "Login jest wymagany."));
            }
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                errors.Add(new ValidationError(nameof(dto.Name), "Imię i nazwisko jest wymagane."));
            }
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            {
                errors.Add(new ValidationError(nameof(dto.Password), "Hasło musi mieć co najmniej 6 znaków."));
            }

            if (errors.Count > 0)
            {
                return OperationResult<Unit>.ValidationFailed(errors);
            }

            var login = dto.Login.Trim();
            var loginTaken = await _context.Set<User>().AnyAsync(u => u.Login == login, cancellationToken);
            
            if (loginTaken)
            {
                return OperationResult<Unit>.Conflict($"Login '{login}' jest już zajęty.");
            }

            var roles = dto.RoleIds.Count == 0
                ? new List<Role>()
                : await _context.Set<Role>()
                    .Where(r => dto.RoleIds.Contains(r.Id))
                    .ToListAsync(cancellationToken);

            if (roles.Count != dto.RoleIds.Count)
            {
                return OperationResult<Unit>.BadRequest("Jedna lub więcej wskazanych ról nie istnieje.");
            }

            var user = new User
            {
                Login = login,
                Name = dto.Name.Trim(),
                PasswordHash = _passwordService.Hash(dto.Password),
                IsArchive = false,
                Roles = roles
            };

            _context.Set<User>().Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            await _notifier.EntityChangedAsync(RealtimeEntities.Users, cancellationToken: cancellationToken);

            return OperationResult<Unit>.Ok(Unit.Value, $"Użytkownik '{user.Login}' został utworzony.");
        }
    }
}
