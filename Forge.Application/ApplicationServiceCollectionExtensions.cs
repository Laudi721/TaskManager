using Microsoft.Extensions.DependencyInjection;

namespace Forge.Application
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddForgeApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly));

            return services;
        }
    }
}
