namespace EventDrive.API;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class StartupDevelopment : Startup
{
    public StartupDevelopment(IConfiguration configuration, IWebHostEnvironment environment)
        : base(configuration, environment)
    {

    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
    }

    public override void Configure(IApplicationBuilder app)
    {
        base.Configure(app);
    }
}