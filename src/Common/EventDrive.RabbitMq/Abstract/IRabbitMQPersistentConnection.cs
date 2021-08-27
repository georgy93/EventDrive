namespace EventDrive.RabbitMq.Abstract
{
    using RabbitMQ.Client;
    using System;

    public interface IRabbitMQPersistentConnection : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateChannel();
    }
}