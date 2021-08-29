namespace EventDrive.RabbitMq.Internal
{
    using RabbitMQ.Client;

    internal static class Extensions
    {
        public static IConnectionFactory ToConnectionFactory(this RabbitMqSettings rabbitMqSettings) => new ConnectionFactory
        {
            UserName = rabbitMqSettings.UserName,
            Password = rabbitMqSettings.Password,
            HostName = rabbitMqSettings.HostName,
            ClientProvidedName = rabbitMqSettings.ClientProvidedConnectionName,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true
        };
    }
}