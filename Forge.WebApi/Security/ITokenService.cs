using Forge.Database.Models;

namespace Forge.WebApi.Security
{
    public record TokenResult(string Token, DateTime ExpiresAtUtc);

    public interface ITokenService
    {
        /// <summary>
        /// Create a signed JWT for the given user.
        /// </summary>
        TokenResult CreateToken(User user);
    }
}
