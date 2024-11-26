namespace EventDrive.RabbitMq.Abstract;

public interface IRabbitMQPersistentConnection : IDisposable
{
    bool IsConnected { get; }

    Task<bool> TryConnectAsync(CancellationToken cancellationToken);

    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken);
}