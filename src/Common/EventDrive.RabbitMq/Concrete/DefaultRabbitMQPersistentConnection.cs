namespace EventDrive.RabbitMq.Concrete;

using Abstract;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using System.IO;
using System.Net.Sockets;

internal class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
    private readonly int _retryCount;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private IConnection _connection;
    private volatile bool _disposed;

    public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<DefaultRabbitMQPersistentConnection> logger, int retryCount = 3)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryCount = retryCount;
    }

    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken)
    {
        if (!IsConnected)
            await TryConnectAsync(cancellationToken);

        return IsConnected
            ? await _connection.CreateChannelAsync(cancellationToken: cancellationToken)
            : throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
    }

    public async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Client is trying to connect");

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await CreateRetryPolicy().ExecuteAsync(async () => _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken));

            if (IsConnected)
            {
                _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
                _connection.CallbackExceptionAsync += OnCallbackExceptionAsync;
                _connection.ConnectionBlockedAsync += OnConnectionBlockedAsync;

                _logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{ConnectionEndpointHostName}' and is subscribed to failure events", _connection.Endpoint.HostName);

                return true;
            }

            _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            _connection?.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex, "{ExMessage}", ex.Message);
        }

        _disposed = true;
    }

    private AsyncRetryPolicy CreateRetryPolicy() => Policy
        .Handle<SocketException>()
        .Or<BrokerUnreachableException>()
        .WaitAndRetryAsync(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
        {
            _logger.LogWarning(ex, "RabbitMQ Client could not connect after {TotalSeconds:n1}s ({ExMessage})", time.TotalSeconds, ex.Message);
        });

    private async Task OnConnectionBlockedAsync(object sender, ConnectionBlockedEventArgs e)
    {
        if (_disposed)
            return;

        _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

        await TryConnectAsync(CancellationToken.None);
    }

    private async Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
    {
        if (_disposed)
            return;

        _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

        await TryConnectAsync(CancellationToken.None);
    }

    private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs reason)
    {
        if (_disposed)
            return;

        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

        await TryConnectAsync(CancellationToken.None);
    }
}
