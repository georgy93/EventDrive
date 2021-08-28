namespace EventDrive.Worker.Host
{
    using Dataflow;
    using DTOs;
    using Microsoft.Extensions.Hosting;
    using RabbitMq.Abstract;
    using RabbitMQ.Client.Events;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    internal class ItemsConsumerBackgroundService : BackgroundService
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly TransformBlock<int, IEnumerable<MyDTO>> _entryJob;

        public ItemsConsumerBackgroundService(IRabbitMQPersistentConnection persistentConnection, ReadStreamBlock readStreamBlock, PersistenceBlock persistenceBlock)
        {
            _persistentConnection = persistentConnection;

            var readStreamJob = readStreamBlock.Build(null);
            var persistenceJob = persistenceBlock.Build(null);

            readStreamJob.LinkTo(persistenceJob, new DataflowLinkOptions { PropagateCompletion = true });

            _entryJob = readStreamJob;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // start a consumer 
            var channel = _persistentConnection.CreateChannel();

            channel.QueueDeclare("items", true, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += (obj, ea) => _entryJob.SendAsync(1, stoppingToken);
           // channel.BasicConsume("items", true, consumer, false, false);
        }
    }
}