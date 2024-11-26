namespace EventDrive.Utils.Health;

using System;
using System.Collections.Generic;
using System.Linq;

public record HealthCheckResponse
{
    public string Status { get; init; }

    public IEnumerable<HealthCheck> Checks { get; init; } = [];

    public TimeSpan TotalDuration { get; init; }
}