namespace EventDrive.RabbitMq.Abstract;

public interface IRabbitMQPersistentConnection : IAsyncDisposable
{
    IConnection Connection { get; }

    bool IsConnected { get; }

    Task<bool> TryConnectAsync(CancellationToken cancellationToken);

    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken);
}