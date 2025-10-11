namespace EventDrive.RabbitMq.Internal;

internal record RabbitMqSettings
{
    public string ClientProvidedConnectionName { get; init; }

    public int CreateConnectionRetryCount { get; init; }

    public string HostName { get; init; }

    public string Password { get; init; }

    public string UserName { get; init; }
}