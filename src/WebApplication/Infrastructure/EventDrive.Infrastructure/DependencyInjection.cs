namespace EventDrive.Infrastructure
{
    using EventDrive.RabbitMq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using StackExchange.Redis;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) => services
            .AddRedisCache(configuration)
            .AddRabbitMq(configuration)
            .AddServices();

        private static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
           return services.AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect("10.0.75.1"));
        }

        private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddRabbitMqMessaging(configuration);
        }

        private static IServiceCollection AddServices(this IServiceCollection services) => services
            .AddSingleton<IRedisService, RedisService>();
    }
}