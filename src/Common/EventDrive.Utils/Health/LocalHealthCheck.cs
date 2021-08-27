namespace EventDrive.Utils.Health
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System.Threading;
    using System.Threading.Tasks;

    public class LocalHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("ok"));
        }
    }
}