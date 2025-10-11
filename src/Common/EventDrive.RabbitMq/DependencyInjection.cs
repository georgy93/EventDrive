namespace EventDrive.RabbitMq;

using Abstract;
using Concrete;
using Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddRabbitMQ(async sp =>
            {
                var connection = sp.GetRequiredService<IRabbitMQPersistentConnection>();

                await connection.TryConnectAsync(CancellationToken.None);

                return connection.Connection;
            }, name: "RabbitMQ HealthCheck", timeout: TimeSpan.FromSeconds(3));

        return services
            .Configure<RabbitMqSettings>(configuration.GetSection(nameof(RabbitMqSettings)))
            .AddSingleton<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>(sp =>
            {
                var rabbitMqSettings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
                var connectionFactory = rabbitMqSettings.ToConnectionFactory();
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                return new DefaultRabbitMQPersistentConnection(connectionFactory, logger, rabbitMqSettings.CreateConnectionRetryCount);
            });
    }
}