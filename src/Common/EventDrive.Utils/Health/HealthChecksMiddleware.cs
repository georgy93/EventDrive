namespace EventDrive.Utils.Health;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

public static class HealthChecksMiddleware
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseCustomHealthChecks(PathString healthCheckPath) => app.UseHealthChecks(healthCheckPath,
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var healthReport = HealthCheckHelper.CreateHealthCheckResponse(report);

                    await context.Response.WriteAsJsonAsync(healthReport, JsonSerializerOptions, context.RequestAborted);
                }
            });
    }
}
