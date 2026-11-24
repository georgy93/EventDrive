namespace EventDrive.Worker.Host.Dataflow;

public sealed class RedisStreamOptions
{
    public const string SectionName = "RedisStreamSettings";

    /// <summary>Shared by all worker instances, so each entry is delivered to only one of them.</summary>
    public string ConsumerGroup { get; set; } = "events_consumer_group";

    /// <summary>Unique per worker instance. Defaults to the machine (container) name.</summary>
    public string ConsumerName { get; set; } = Environment.MachineName;

    /// <summary>Maximum number of entries read from Redis and persisted in one batch.</summary>
    public int BatchSize { get; set; } = 500;

    /// <summary>
    /// How long an entry may stay unacknowledged before another read reclaims it (crashed worker or failed insert).
    /// Should be longer than the time a batch can take to be persisted; inserts are idempotent, so a shorter
    /// value only costs extra work, not duplicate rows.
    /// </summary>
    public TimeSpan ClaimIdleTime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// How many times an entry may be delivered before it is moved to the dead-letter stream instead of being retried.
    /// </summary>
    public int MaxDeliveryAttempts { get; set; } = 5;
}
