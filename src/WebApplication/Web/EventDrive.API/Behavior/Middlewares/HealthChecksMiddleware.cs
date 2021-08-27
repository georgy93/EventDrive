namespace EventDrive.API.Behavior.Middlewares
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Http;
    using System.Net.Mime;
    using Utils.Extensions;
    using Utils.Helpers;

    public static class HealthChecksMiddleware
    {
        public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app, PathString healthCheckPath) => app
            .UseHealthChecks(healthCheckPath, new HealthCheckOptions()
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = MediaTypeNames.Application.Json;

                    var hcReport = HealthCheckHelper.CreateHealthCheckResponse(report).Beautify();

                    await context.Response.WriteAsync(hcReport);
                }
            });
    }
}
