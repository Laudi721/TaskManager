using System.Security.Claims;
using Forge.Common.Dtos;
using Forge.Database;
using Forge.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forge.Desk.WebApi.Controllers
{
    /// <summary>
    /// Preferencje per-user (motyw, w przyszłości język/density/itp.). Wszystkie endpointy
    /// dotyczą obecnie zalogowanego użytkownika — userId czytamy z JWT, klient nie może
    /// wskazać innego konta.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/user-settings")]
    public class UserSettingsController : ControllerBase
    {
        private const int MaxThemeLength = 64;

        private readonly ForgeDbContext _db;

        public UserSettingsController(ForgeDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<UserPreferencesDto>> GetMine(CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var prefs = await _db.Set<User>()
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new UserPreferencesDto
                {
                    ThemePreference = u.ThemePreference
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (prefs == null)
            {
                return NotFound();
            }

            return Ok(prefs);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMine(
            [FromBody] UserPreferencesDto dto,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            // Whitelist motywów jest po stronie frontu; tu tylko limitujemy długość, żeby ktoś
            // nie zapchał kolumny śmieciem i nie wepchnął tam HTML/script-a.
            user.ThemePreference = SanitizeTheme(dto.ThemePreference);

            await _db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        private bool TryGetUserId(out int userId)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out userId);
        }

        private static string? SanitizeTheme(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }
            var trimmed = raw.Trim();
            return trimmed.Length > MaxThemeLength ? trimmed[..MaxThemeLength] : trimmed;
        }
    }
}
