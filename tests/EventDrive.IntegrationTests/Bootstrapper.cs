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

            services
              .AddRefitClient<IEventDriveAPIClient>(new RefitSettings())
              .ConfigureHttpClient((sp, client) =>
              {
                  var apuUri = sp.GetRequiredService<IConfiguration>().GetSection("EventDriveAPIUrl").Value;

                  client.BaseAddress = new Uri(apuUri);
                  client.Timeout = TimeSpan.FromSeconds(10);
              });

            return services.AddSingleton<IConfiguration>(configuration);
        }
    }
}