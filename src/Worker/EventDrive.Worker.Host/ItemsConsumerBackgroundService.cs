namespace EventDrive.Worker.Host
{
    using Dataflow;
    using DTOs;
    using DTOs.IntegrationEvents;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RabbitMq.Abstract;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    internal class ItemsConsumerBackgroundService : BackgroundService
    {
        const string BROKER_NAME = "integrationEvents";

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<ItemsConsumerBackgroundService> _logger;
        private readonly TransformBlock<int, IEnumerable<MyDTO>> _entryJob;

        private IModel _consumerChanel;

        public ItemsConsumerBackgroundService(IRabbitMQPersistentConnection persistentConnection,
                                              ILogger<ItemsConsumerBackgroundService> logger,
                                              ReadStreamBlock readStreamBlock,
                                              PersistenceBlock persistenceBlock)
        {
            _persistentConnection = persistentConnection;
            _logger = logger;

            var readStreamJob = readStreamBlock.Build(new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 30,
                MaxDegreeOfParallelism = 1,
                EnsureOrdered = true
            });

            var persistenceJob = persistenceBlock.Build(new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 10,
                MaxDegreeOfParallelism = 1, // If the order of insertion does not matter, this block can be paralelized.
                EnsureOrdered = true
            });

            readStreamJob.LinkTo(persistenceJob, new DataflowLinkOptions { PropagateCompletion = true });

            _entryJob = readStreamJob;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumerChanel = _persistentConnection.CreateChannel();
            _consumerChanel.ExchangeDeclare(BROKER_NAME, "direct");

            var queueName = _consumerChanel.QueueDeclare();
            _consumerChanel.QueueBind(queueName, BROKER_NAME, typeof(ItemsAddedToRedisIntegrationEvent).Name);

            var consumer = new AsyncEventingBasicConsumer(_consumerChanel);

            consumer.Received += async (obj, ea) =>
            {
                try
                {
                    await _entryJob.SendAsync(1, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occured");
                }
            };

            _consumerChanel.BasicConsume(queueName, true, consumer);

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (_consumerChanel is { IsClosed: true })
                _consumerChanel.Close();

            return Task.CompletedTask;
        }
    }
}