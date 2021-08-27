namespace EventDrive.Utils.Health
{
    using System;

    public record HealthCheck
    {
        public string Status { get; init; }

        public string Component { get; init; }

        public string Description { get; init; }

        public TimeSpan Duration { get; init; }

        public Exception Error { get; init; }
    }
}