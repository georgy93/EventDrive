namespace EventDrive.Worker.Host
{
    using Dataflow;
    using DTOs;
    using DTOs.IntegrationEvents;
    using Microsoft.Extensions.Hosting;
    using RabbitMq.Abstract;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    internal class ItemsConsumerBackgroundService : BackgroundService
    {
        const string BROKER_NAME = "integrationEvents";

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly TransformBlock<int, IEnumerable<MyDTO>> _entryJob;

        public ItemsConsumerBackgroundService(IRabbitMQPersistentConnection persistentConnection, ReadStreamBlock readStreamBlock, PersistenceBlock persistenceBlock)
        {
            _persistentConnection = persistentConnection;

            var readStreamJob = readStreamBlock.Build(new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 30,
                MaxDegreeOfParallelism = 1,
                EnsureOrdered = true
            });

            var persistenceJob = persistenceBlock.Build(new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 10,
                MaxDegreeOfParallelism = 1,
                EnsureOrdered = true
            });

            readStreamJob.LinkTo(persistenceJob, new DataflowLinkOptions { PropagateCompletion = true });

            _entryJob = readStreamJob;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // start a consumer 
            var channel = _persistentConnection.CreateChannel();

            channel.ExchangeDeclare(BROKER_NAME, "direct");

            var queueName = channel.QueueDeclare();
            channel.QueueBind(queueName, BROKER_NAME, typeof(ItemsAddedToRedisIntegrationEvent).Name);

            var consumer = new AsyncEventingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);

            consumer.Received += (obj, ea) =>
            {
                return _entryJob.SendAsync(1, stoppingToken);
            };
            // channel.BasicConsume("items", true, consumer, false, false);

            return Task.CompletedTask;
        }
    }
}