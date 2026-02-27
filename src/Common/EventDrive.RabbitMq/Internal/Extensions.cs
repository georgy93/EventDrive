namespace EventDrive.RabbitMq.Internal;

internal static class Extensions
{
    extension(RabbitMqSettings rabbitMqSettings)
    {
        public IConnectionFactory ToConnectionFactory() => new ConnectionFactory
        {
            UserName = rabbitMqSettings.UserName,
            Password = rabbitMqSettings.Password,
            HostName = rabbitMqSettings.HostName,
            ClientProvidedName = rabbitMqSettings.ClientProvidedConnectionName,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };
    }
}