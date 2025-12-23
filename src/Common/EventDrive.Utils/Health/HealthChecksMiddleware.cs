namespace EventDrive.Utils.Health;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

public static class HealthChecksMiddleware
{
    private static JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app, PathString healthCheckPath) => app.UseHealthChecks(healthCheckPath, new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            var healthReport = HealthCheckHelper.CreateHealthCheckResponse(report);

            await context.Response.WriteAsJsonAsync(healthReport, jsonSerializerOptions, context.RequestAborted);
        }
    });
}
