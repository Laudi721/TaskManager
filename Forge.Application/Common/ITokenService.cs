using Forge.Database.Models;

namespace Forge.Application.Common
{
    public record TokenResult(string Token, DateTime ExpiresAtUtc);

    public interface ITokenService
    {
        /// <summary>
        /// Create a signed JWT for the given user with the given sessionId embedded as 'sid' claim.
        /// </summary>
        TokenResult CreateToken(User user, string sessionId);

        /// <summary>
        /// Token lifetime configured via Jwt:ExpiresMinutes. Used to align session TTL with token TTL.
        /// </summary>
        TimeSpan TokenLifetime { get; }
    }
}
