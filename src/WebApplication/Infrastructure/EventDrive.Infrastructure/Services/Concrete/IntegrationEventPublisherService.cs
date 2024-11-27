namespace EventDrive.Infrastructure.Services.Concrete;

using Abstract;
using DTOs.IntegrationEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RabbitMq.Abstract;

public class IntegrationEventPublisherService : IIntegrationEventPublisherService
{
    const string BROKER_NAME = "integrationEvents";

    private readonly ILogger<IntegrationEventPublisherService> _logger;
    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly int _retryCount;

    public IntegrationEventPublisherService(ILogger<IntegrationEventPublisherService> logger,
                                            IConfiguration configuration,
                                            IRabbitMQPersistentConnection persistentConnection)
    {
        _logger = logger;
        _persistentConnection = persistentConnection;
        _retryCount = int.Parse(configuration.GetSection("IntegrationEventsSettings:PublishRetryCount").Value);
    }

    public async Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (!_persistentConnection.IsConnected)
            await _persistentConnection.TryConnectAsync(cancellationToken);

        var policy = Policy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
            {
                _logger.LogWarning(ex, "Could not publish event: {IntegrationEventId} after {TotalSeconds:n1}s ({ExMessage})", integrationEvent.Id, time.TotalSeconds, ex.Message);
            });

        using var channel = await _persistentConnection.CreateChannelAsync(cancellationToken); // or have a single channel and lock on every write

        await channel.ExchangeDeclareAsync(exchange: BROKER_NAME, type: "direct", durable: false, autoDelete: false, cancellationToken: cancellationToken);

        await policy.ExecuteAsync(async () =>
        {
            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            _logger.LogDebug("Publishing event to RabbitMQ: {IntegrationEventId}", integrationEvent.Id);

            await channel.BasicPublishAsync(
                exchange: BROKER_NAME,
                routingKey: integrationEvent.GetType().Name,
                mandatory: true,
                basicProperties: properties,
                body: GetMessageBytes(integrationEvent),
                cancellationToken: cancellationToken);
        });
    }

    private static ReadOnlyMemory<byte> GetMessageBytes(IntegrationEvent integrationEvent)
    {
        var message = JsonConvert.SerializeObject(integrationEvent);

        return Encoding.UTF8.GetBytes(message);
    }
}