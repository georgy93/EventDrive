namespace EventDrive.API
{
    using Behavior.Filters;
    using Behavior.Settings;
    using FluentValidation.AspNetCore;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Serialization;
    using Swagger;
    using Utils.Health;
    using Validations;

    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration config) => services
               .AddPresentationConfigurations(config)
               .AddCustomWebApi()
               .AddHttpContextAccessor()
               .AddSwagger();

        private static IServiceCollection AddCustomWebApi(this IServiceCollection services)
        {
            services.AddControllers(opts =>
            {
                opts.Filters.Add<ModelValidationFilter>();
            })
            .AddNewtonsoftJson(jsonOptions =>
            {
                jsonOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            })
            .AddFluentValidation(mvcConfig =>
            {
                mvcConfig.RegisterValidatorsFromAssemblyContaining(typeof(BaseValidator<>));
            });

            services
                .AddHealthChecks()
                .AddCheck<LocalHealthCheck>("local-hc");

            return services
                 .Configure<ApiBehaviorOptions>(options =>
                 {
                     // if we use [ApiController] it will internally use its own ModelState filter
                     // and we will not reach our custom Validation Filter
                     options.SuppressModelStateInvalidFilter = true;
                 })
                 .AddCors(options =>
                 {
                     options.AddPolicy("AllowAll",
                         builder => builder
                             .AllowAnyOrigin()
                             .AllowAnyMethod()
                             .AllowAnyHeader()
                         );
                 });
        }

        private static IServiceCollection AddPresentationConfigurations(this IServiceCollection services, IConfiguration config) => services
            .Configure<ErrorHandlingSettings>(config.GetSection(nameof(ErrorHandlingSettings)));
    }
}