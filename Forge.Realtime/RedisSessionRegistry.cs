using StackExchange.Redis;

namespace Forge.Realtime;

internal sealed class RedisSessionRegistry : ISessionRegistry
{
    private const string KeyPrefix = "forge:session:";
    private readonly IConnectionMultiplexer _redis;

    public RedisSessionRegistry(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<string> StartAsync(int userId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var db = _redis.GetDatabase();
        await db.StringSetAsync(Key(userId), sessionId, ttl);
        return sessionId;
    }

    public async Task<bool> IsCurrentAsync(int userId, string sessionId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var stored = await db.StringGetAsync(Key(userId));
        return stored.HasValue && stored.ToString() == sessionId;
    }

    public async Task EndAsync(int userId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(Key(userId));
    }

    private static string Key(int userId) => KeyPrefix + userId;
}
