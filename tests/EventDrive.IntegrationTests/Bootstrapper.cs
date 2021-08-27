namespace EventDrive.IntegrationTests
{
    using Common;
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

            services
              .AddRefitClient<IEventDriveAPIClient>(new RefitSettings())
              .ConfigureHttpClient(client =>
              {
                  client.BaseAddress = new Uri("https://www.githudb.com"); // TODO: Change to actual url
                  client.Timeout = TimeSpan.FromSeconds(3);
              });

            return services;
        }
    }
}