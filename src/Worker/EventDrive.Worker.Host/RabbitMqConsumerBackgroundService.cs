namespace EventDrive.Worker.Host
{
    using Microsoft.Extensions.Hosting;
    using RabbitMq.Abstract;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class RabbitMqConsumerBackgroundService : BackgroundService
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;

        public RabbitMqConsumerBackgroundService(IRabbitMQPersistentConnection persistentConnection)
        {
            _persistentConnection = persistentConnection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(12);
        }
    }
}