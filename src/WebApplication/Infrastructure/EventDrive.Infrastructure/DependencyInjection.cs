﻿namespace EventDrive.Infrastructure
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using RabbitMq;
    using Services;
    using StackExchange.Redis;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) => services
            .AddRedis(configuration)
            .AddRabbitMqMessaging(configuration)
            .AddServices();

        private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var redisHost = configuration.GetSection("RedisSettings:Host").Value;
            var redisReconnectStrategy = bool.Parse(configuration.GetSection("redisSettings:AbortOnConnectFail").Value);

            services
                .AddHealthChecks()
                .AddRedis(redisHost, "redis");

            var connectionOpts = new ConfigurationOptions
            {
                AbortOnConnectFail = redisReconnectStrategy,
                EndPoints = { redisHost }
            };

            return services
                .AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect(connectionOpts));
        }

        private static IServiceCollection AddServices(this IServiceCollection services) => services
            .AddSingleton<IEventStreamService, RedisService>()
            .AddSingleton<IntegrationEventPublisherService>();
    }
}