namespace EventDrive.Worker.Infrastructure
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using RabbitMq;
    using Services.Background;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) => services
            .AddBackgroundServices()
            .AddRabbitMqMessaging(configuration);

        private static IServiceCollection AddBackgroundServices(this IServiceCollection services) => services
             .AddHostedService<RabbitMqConsumerBackgroundService>();
    }
}