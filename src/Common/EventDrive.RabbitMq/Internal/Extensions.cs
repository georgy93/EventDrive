namespace EventDrive.RabbitMq.Internal;

internal static class Extensions
{
    public static IConnectionFactory ToConnectionFactory(this RabbitMqSettings rabbitMqSettings) => new ConnectionFactory
    {
        UserName = rabbitMqSettings.UserName,
        Password = rabbitMqSettings.Password,
        HostName = rabbitMqSettings.HostName,
        ClientProvidedName = rabbitMqSettings.ClientProvidedConnectionName,
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
    };
}