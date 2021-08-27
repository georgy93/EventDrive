namespace EventDrive.API
{
    using Behavior.Middlewares;
    using Infrastructure;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Swagger;

    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services
                  .AddInfrastructure(Configuration)
                  .AddPresentation(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // The order of addition matters!
            app.UseSwagger(Configuration)
               .UseHttpsRedirection()
               .UseStaticFiles()
               .UseRouting()
               .UseCors("AllowAll")
               .UseCustomHealthChecks("/health")
               .UseMiddleware<GlobalExceptionHandlingMiddleware>()
               .UseEndpoints(endpoints =>
               {
                   endpoints.MapControllers();
               });
        }     
    }
}