namespace EventDrive.API.Swagger
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.IO;
    using System.Reflection;

    public static class SwaggerSetup
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services) => services
            .AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EventDrive API",
                    Description = "This is just a playground application",
                    Version = "v1"
                });
                opt.ExampleFilters();
                opt.IncludeXmlComments(GetCommentsPath());

                /* <PropertyGroup>
                    <GenerateDocumentationFile>true</GenerateDocumentationFile>
                    <NoWarn>$(NoWarn);1591</NoWarn>
                  </PropertyGroup>
                */
            })
            .AddSwaggerExamplesFromAssemblyOf<Startup>();

        private static string GetCommentsPath()
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            return xmlPath;
        }
    }
}