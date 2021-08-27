namespace EventDrive.API.Swagger
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;

    public static class SwaggerMiddlewareBuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app, IConfiguration configuration)
        {
            var swaggerSettings = new SwaggerSettings();
            configuration.GetSection(nameof(SwaggerSettings)).Bind(swaggerSettings);

            return app
                .UseSwagger(opt =>
                {
                    opt.RouteTemplate = swaggerSettings.JsonRoute;
                })
                .UseSwaggerUI(opt =>
                {
                    opt.SwaggerEndpoint(swaggerSettings.UIEndpoint, swaggerSettings.Description);
                });
        }
    }
}