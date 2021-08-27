namespace EventDrive.Infrastructure
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using StackExchange.Redis;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) => services
            .AddRedisCache(configuration)
            .AddServices();

        private static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
           return services.AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect("10.0.75.1"));
        }

        private static IServiceCollection AddServices(this IServiceCollection services) => services
            .AddSingleton<IRedisService, RedisService>();
    }
}