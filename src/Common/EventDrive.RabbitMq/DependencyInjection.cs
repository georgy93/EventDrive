namespace EventDrive.RabbitMq
{
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
                .AddRabbitMQ(sp =>
                {
                    var rabbitMqSettings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
                    var connectionFactory = rabbitMqSettings.ToConnectionFactory();

                    connectionFactory.ClientProvidedName = $"{rabbitMqSettings.ClientProvidedConnectionName}_HealthCheck";

                    return connectionFactory;
                });

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
}
