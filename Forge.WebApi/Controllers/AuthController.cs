using Microsoft.AspNetCore.Mvc;
using TaskManager.Common.Interfaces;
using TaskManager.Database;
using TaskManager.Database.Models;
using TaskManager.WebApi.Dtos;
using TaskManager.WebApi.Security;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly TaskManagerDbContext _db;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public AuthController(TaskManagerDbContext db, IPasswordService passwordService, ITokenService tokenService)
        {
            _db = db;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse { Success = false, Message = "Login and password are required." });
            }

            var user = await _db.Set<User>()
                .FirstOrDefaultAsync(u => u.Login == request.Login && !u.IsArchive);

            if (user == null || !_passwordService.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new LoginResponse { Success = false, Message = "Invalid login or password." });
            }

            var token = _tokenService.CreateToken(user);

            return Ok(new LoginResponse
            {
                Success = true,
                UserId = user.Id,
                Name = user.Name,
                Login = user.Login,
                Token = token.Token,
                ExpiresAtUtc = token.ExpiresAtUtc
            });
        }
    }
}
