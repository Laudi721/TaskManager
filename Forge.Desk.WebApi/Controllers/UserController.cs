using System.Security.Claims;
using Forge.Common.Dtos;
using Forge.Common.Interfaces;
using Forge.Database;
using Forge.Database.Models;
using Forge.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forge.Desk.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly ForgeDbContext _db;
        private readonly IPasswordService _passwordService;
        private readonly IRealtimeNotifier _notifier;

        public UserController(
            ForgeDbContext db,
            IPasswordService passwordService,
            IRealtimeNotifier notifier)
        {
            _db = db;
            _passwordService = passwordService;
            _notifier = notifier;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserDto>>> Get(CancellationToken cancellationToken)
        {
            var users = await _db.Set<User>()
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

            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create(
            [FromBody] UserCreateDto dto,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.Login))
            {
                return BadRequest(new { message = "Login jest wymagany." });
            }
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Imię i nazwisko jest wymagane." });
            }
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            {
                return BadRequest(new { message = "Hasło musi mieć co najmniej 6 znaków." });
            }

            var login = dto.Login.Trim();
            var loginTaken = await _db.Set<User>().AnyAsync(u => u.Login == login, cancellationToken);
            if (loginTaken)
            {
                return Conflict(new { message = $"Login '{login}' jest już zajęty." });
            }

            var roles = dto.RoleIds.Count == 0
                ? new List<Role>()
                : await _db.Set<Role>()
                    .Where(r => dto.RoleIds.Contains(r.Id))
                    .ToListAsync(cancellationToken);

            if (roles.Count != dto.RoleIds.Count)
            {
                return BadRequest(new { message = "Jedna lub więcej wskazanych ról nie istnieje." });
            }

            var user = new User
            {
                Login = login,
                Name = dto.Name.Trim(),
                PasswordHash = _passwordService.Hash(dto.Password),
                IsArchive = false,
                Roles = roles
            };

            _db.Set<User>().Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            await _notifier.EntityChangedAsync(RealtimeEntities.Users, cancellationToken: cancellationToken);

            var result = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Name = user.Name,
                IsArchive = user.IsArchive,
                Roles = roles.Select(r => r.Name).ToList()
            };

            return CreatedAtAction(nameof(Get), new { id = user.Id }, result);
        }

        [HttpPost("{id:int}/archive")]
        public async Task<IActionResult> ToggleArchive(int id, CancellationToken cancellationToken)
        {
            if (IsCurrentUser(id))
            {
                return BadRequest(new { message = "Nie można zarchiwizować własnego konta." });
            }

            var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            user.IsArchive = !user.IsArchive;
            await _db.SaveChangesAsync(cancellationToken);

            await _notifier.EntityChangedAsync(RealtimeEntities.Users, cancellationToken: cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            if (IsCurrentUser(id))
            {
                return BadRequest(new { message = "Nie można usunąć własnego konta." });
            }

            var user = await _db.Set<User>()
                .Include(u => u.Roles)
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Tasks.Count > 0)
            {
                return Conflict(new { message = "Nie można usunąć użytkownika, który ma przypisane zadania. Najpierw zarchiwizuj." });
            }

            _db.Set<User>().Remove(user);
            await _db.SaveChangesAsync(cancellationToken);

            await _notifier.EntityChangedAsync(RealtimeEntities.Users, cancellationToken: cancellationToken);

            return NoContent();
        }

        private bool IsCurrentUser(int userId)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var current) && current == userId;
        }
    }
}
