namespace EventDrive.Worker.Infrastructure
{
    using Services.Background;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) => services
            .AddBackgroundServices();

        private static IServiceCollection AddBackgroundServices(this IServiceCollection services) => services
             .AddHostedService<RabbitMqConsumerBackgroundService>();
    }
}