namespace EventDrive.Worker.Host;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using RabbitMq;
using StackExchange.Redis;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration) => services
            .AddRedis(configuration)
            .AddDataBase(configuration)
            .AddRabbitMqMessaging(configuration);

        private IServiceCollection AddRedis(IConfiguration configuration)
        {
            var redisHost = configuration.GetSection("RedisSettings:Host").Value;
            var redisReconnectStrategy = bool.Parse(configuration.GetSection("redisSettings:AbortOnConnectFail").Value);

            services
                .AddHealthChecks()
                .AddRedis(redisHost, "Redis HealthCheck", timeout: TimeSpan.FromSeconds(2));

            return services
                .AddSingleton<IConnectionMultiplexer>(x =>
                {
                    var connectionOpts = new ConfigurationOptions
                    {
                        AbortOnConnectFail = redisReconnectStrategy,
                        EndPoints = { redisHost }
                    };

                    return ConnectionMultiplexer.Connect(connectionOpts);
                });
        }

        private IServiceCollection AddDataBase(IConfiguration configuration)
        {
            var section = configuration.GetSection(PersistenceOptions.SectionName);
            var provider = section.Get<PersistenceOptions>()?.Provider ?? new PersistenceOptions().Provider;

            services.Configure<PersistenceOptions>(section);

            // Each provider has its own entry under ConnectionStrings, named after it ("MsSql", "Postgres").
            var connectionString = configuration.GetConnectionString(provider.ToString());

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"{PersistenceOptions.SectionName}:Provider is {provider}, but ConnectionStrings:{provider} is not set.");

            var healthChecks = services.AddHealthChecks();
            var healthCheckTimeout = TimeSpan.FromSeconds(3);

            switch (provider)
            {
                case DatabaseProvider.MsSql:
                    healthChecks.AddSqlServer(connectionString, name: "SQL Server HealthCheck", timeout: healthCheckTimeout);
                    services.AddSingleton<IItemsPersistenceStrategy>(new MsSqlItemsPersistenceStrategy(connectionString));
                    break;

                case DatabaseProvider.Postgres:
                    healthChecks.AddNpgSql(connectionString, name: "PostgreSQL HealthCheck", timeout: healthCheckTimeout);
                    services.AddNpgsqlDataSource(connectionString);
                    services.AddSingleton<IItemsPersistenceStrategy, PostgresItemsPersistenceStrategy>();
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported {PersistenceOptions.SectionName}:Provider '{provider}'.");
            }

            return services;
        }
    }
}