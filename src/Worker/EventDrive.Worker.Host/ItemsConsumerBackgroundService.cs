namespace EventDrive.Worker.Host;

using Dataflow;
using DTOs;
using DTOs.IntegrationEvents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Abstract;
using System.Threading.Tasks.Dataflow;

internal class ItemsConsumerBackgroundService : BackgroundService
{
    const string BROKER_NAME = "integrationEvents";

    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly ILogger<ItemsConsumerBackgroundService> _logger;
    private readonly TransformBlock<int, IEnumerable<MyDto>> _entryJob;

    private IChannel _consumerChanel;

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumerChanel = await _persistentConnection.CreateChannelAsync(stoppingToken);
        await _consumerChanel.ExchangeDeclareAsync(BROKER_NAME, "direct", cancellationToken: stoppingToken);

        var queueName = await _consumerChanel.QueueDeclareAsync(cancellationToken: stoppingToken);
        await _consumerChanel.QueueBindAsync(queueName, BROKER_NAME, typeof(ItemsAddedToRedisIntegrationEvent).Name, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_consumerChanel);

        consumer.ReceivedAsync += async (obj, ea) =>
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

        await _consumerChanel.BasicConsumeAsync(queueName, true, consumer, cancellationToken: stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consumerChanel is { IsClosed: true })
            await _consumerChanel.CloseAsync(cancellationToken);
    }
}