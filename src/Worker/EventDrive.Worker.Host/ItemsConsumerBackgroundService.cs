namespace EventDrive.Worker.Host;

using Dataflow;
using DTOs.IntegrationEvents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMq.Abstract;
using System.Text;
using System.Threading.Tasks.Dataflow;

internal class ItemsConsumerBackgroundService : BackgroundService
{
    const string BROKER_NAME = "integrationEvents";

    private readonly ILogger<ItemsConsumerBackgroundService> _logger;
    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly ReadStreamBlock _readStreamBlock;
    private readonly PersistenceBlock _persistenceBlock;
    private readonly RedisStreamOptions _streamOptions;
    private ITargetBlock<int> _entryJob;
    private Task _pipelineCompletion;
    private IChannel _consumerChanel;

    public ItemsConsumerBackgroundService(ILogger<ItemsConsumerBackgroundService> logger, 
                                          IRabbitMQPersistentConnection persistentConnection,
                                          ReadStreamBlock readStreamBlock,
                                          PersistenceBlock persistenceBlock,
                                          IOptions<RedisStreamOptions> streamOptions)
    {
        _logger = logger;
        _streamOptions = streamOptions.Value;
        _readStreamBlock = readStreamBlock;
        _persistenceBlock = persistenceBlock;
        _persistentConnection = persistentConnection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeBlocksAsync();

        (var queueName, var consumer) = await InitializeConsumerAsync(stoppingToken);

        consumer.ReceivedAsync += async (obj, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                _logger.LogInformation("Event Received: {EventJson}", json);

                await _entryJob.SendAsync(1, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured");
            }
        };

        await _consumerChanel.BasicConsumeAsync(queueName, true, consumer, cancellationToken: stoppingToken);

        await TriggerPeriodicallyAsync(stoppingToken);
    }

    /// <summary>
    /// Reads the stream on a timer as well, so entries left pending by a failed insert or a crashed worker are
    /// retried (or dead-lettered) even when no new events arrive.
    /// </summary>
    private async Task TriggerPeriodicallyAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_streamOptions.ClaimIdleTime);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                // Post, not SendAsync: if the pipeline is busy or completing, a read is already coming.
                _entryJob.Post(1);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutting down.
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consumerChanel is { IsOpen: true })
            await _consumerChanel.CloseAsync(cancellationToken);

        // Let batches already read finish persisting and get acknowledged. Anything cut off by the host's
        // shutdown timeout stays pending in Redis and is reclaimed by the next run.
        if (_entryJob is not null)
        {
            _entryJob.Complete();

            try
            {
                await _pipelineCompletion.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Shutdown timed out before the pipeline finished; unacknowledged entries will be retried");
            }
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task InitializeBlocksAsync()
    {
        var readStreamJob = await _readStreamBlock.BuildAsync(new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 30,
            MaxDegreeOfParallelism = 1,
            EnsureOrdered = true
        });

        var persistenceJob = _persistenceBlock.Build(new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 10,
            MaxDegreeOfParallelism = 1, // If the order of insertion does not matter, this block can be paralelized.
            EnsureOrdered = true
        });

        var acknowledgeJob = _readStreamBlock.BuildAcknowledgeBlock(new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 10,
            MaxDegreeOfParallelism = 1
        });

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        readStreamJob.LinkTo(persistenceJob, linkOptions);
        persistenceJob.LinkTo(acknowledgeJob, linkOptions);

        _entryJob = readStreamJob;
        _pipelineCompletion = acknowledgeJob.Completion;
    }

    private async Task<(QueueDeclareOk queueName, AsyncEventingBasicConsumer consumer)> InitializeConsumerAsync(CancellationToken stoppingToken)
    {
        await _persistentConnection.TryConnectAsync(stoppingToken);

        _consumerChanel = await _persistentConnection.CreateChannelAsync(stoppingToken);

        await _consumerChanel.ExchangeDeclareAsync(BROKER_NAME, "direct", cancellationToken: stoppingToken);

        var queueName = await _consumerChanel.QueueDeclareAsync(cancellationToken: stoppingToken);

        await _consumerChanel.QueueBindAsync(queueName, BROKER_NAME, typeof(ItemsAddedToRedisIntegrationEvent).Name, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_consumerChanel);

        return (queueName, consumer);
    }
}