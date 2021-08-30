namespace EventDrive.Infrastructure.Services
{
    using DTOs.IntegrationEvents;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Polly;
    using RabbitMq.Abstract;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Exceptions;
    using System;
    using System.Net.Sockets;
    using System.Text;

    public interface IIntegrationEventPublisherService
    {
        void Publish(IntegrationEvent integrationEvent);
    }

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

        public void Publish(IntegrationEvent integrationEvent)
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, $"Could not publish event: {integrationEvent.Id} after {time.TotalSeconds:n1}s ({ex.Message})");
                });

            using var channel = _persistentConnection.CreateChannel(); // or have a single channel and lock on every write

            channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");

            policy.Execute(() =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                _logger.LogDebug($"Publishing event to RabbitMQ: {integrationEvent.Id}");

                channel.BasicPublish(
                    exchange: BROKER_NAME,
                    routingKey: integrationEvent.GetType().Name,
                    mandatory: true,
                    basicProperties: properties,
                    body: GetMessageBytes(integrationEvent));
            });
        }

        private static ReadOnlyMemory<byte> GetMessageBytes(IntegrationEvent integrationEvent)
        {
            var message = JsonConvert.SerializeObject(integrationEvent);

            return Encoding.UTF8.GetBytes(message);
        }
    }
}