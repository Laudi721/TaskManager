using Forge.Common.Dtos;
using Forge.Database;
using Forge.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forge.Desk.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly ForgeDbContext _db;

        public RolesController(ForgeDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RoleDto>>> Get(CancellationToken cancellationToken)
        {
            var roles = await _db.Set<Role>()
                .AsNoTracking()
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    UsersCount = r.Users.Count
                })
                .ToListAsync(cancellationToken);

            return Ok(roles);
        }
    }
}
