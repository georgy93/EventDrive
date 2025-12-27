namespace EventDrive.Utils.Health;

using System;
using System.Collections.Generic;

public record HealthCheckResponse
{
    public string Status { get; init; }

    public IEnumerable<HealthCheck> Checks { get; init; } = [];

    public TimeSpan TotalDuration { get; init; }
}