namespace EventDrive.Worker.Host;

using EventDrive.Worker.Host.Dataflow;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.Health;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<LocalHealthCheck>("Local HealthCheck");

        services
            .AddHostedService<ItemsConsumerBackgroundService>()
            .AddSingleton<ReadStreamBlock>()
            .AddSingleton<PersistenceBlock>()
            .AddInfrastructure(Configuration); // normally the infrastructure layer would be another class library
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public virtual void Configure(IApplicationBuilder app)
    {
        app.UseCustomHealthChecks("/health");
    }
}