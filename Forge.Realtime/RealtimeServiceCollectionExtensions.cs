using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Forge.Realtime;

public static class RealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddForgeRealtime(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RealtimeOptions>(configuration.GetSection(RealtimeOptions.SectionName));

        var options = configuration.GetSection(RealtimeOptions.SectionName).Get<RealtimeOptions>() ?? new RealtimeOptions();

        var signalR = services.AddSignalR();
        services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

        if (!string.IsNullOrWhiteSpace(options.Redis))
        {
            // Shared multiplexer used by both the session registry and any future direct Redis access.
            // The SignalR backplane below uses its own internal connection.
            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(options.Redis));

            services.AddSingleton<ISessionRegistry, RedisSessionRegistry>();

            signalR.AddStackExchangeRedis(options.Redis, redisOptions =>
            {
                redisOptions.Configuration.ChannelPrefix = RedisChannel.Literal(options.ChannelPrefix);
            });
        }
        else
        {
            services.AddSingleton<ISessionRegistry, InMemorySessionRegistry>();
        }

        services.AddSingleton<IRealtimeNotifier, HubRealtimeNotifier>();

        return services;
    }

    public static IEndpointRouteBuilder MapForgeRealtime(this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<RealtimeOptions>>().Value;
        endpoints.MapHub<NotificationsHub>(options.HubPath);
        return endpoints;
    }
}
