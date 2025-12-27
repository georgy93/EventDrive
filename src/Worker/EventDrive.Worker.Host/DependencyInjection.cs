namespace EventDrive.Worker.Host;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq;
using StackExchange.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) => services
        .AddRedis(configuration)
        .AddDataBase(configuration)
        .AddRabbitMqMessaging(configuration);

    private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisHost = configuration.GetSection("RedisSettings:Host").Value;

        services
            .AddHealthChecks()
            .AddRedis(redisHost, "Redis HealthCheck", timeout: TimeSpan.FromSeconds(2));

        return services
            .AddSingleton<IConnectionMultiplexer>(x =>
            {
                var redisReconnectStrategy = bool.Parse(configuration.GetSection("redisSettings:AbortOnConnectFail").Value);

                var connectionOpts = new ConfigurationOptions
                {
                    AbortOnConnectFail = redisReconnectStrategy,
                    EndPoints = { redisHost }
                };

                return ConnectionMultiplexer.Connect(connectionOpts);
            });
    }

    private static IServiceCollection AddDataBase(this IServiceCollection services, IConfiguration configuration)
    {
        services
           .AddHealthChecks()
           .AddSqlServer(sp => configuration.GetConnectionString("DefaultConnection"), name: "SQL Server HealthCheck", timeout: TimeSpan.FromSeconds(3));

        return services;
    }
}