namespace EventDrive.IntegrationTests
{
    using Common;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;
    using SolidToken.SpecFlow.DependencyInjection;
    using System;

    public static class Bootstrapper
    {
        [ScenarioDependencies]
        public static IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var apiUrl = configuration.GetSection("EventDriveAPIUrl").Value;

            services
              .AddRefitClient<IEventDriveAPIClient>(new RefitSettings())
              .ConfigureHttpClient(client =>
              {
                  client.BaseAddress = new Uri(apiUrl);
                  client.Timeout = TimeSpan.FromSeconds(10);
              });

            return services;
        }
    }
}