namespace EventDrive.Worker.Host.Dataflow;

using DTOs;
using StackExchange.Redis;
using System.Threading.Tasks.Dataflow;

public class ReadStreamBlock
{
    private readonly ILogger<ReadStreamBlock> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    private readonly string _consumerGroupId;
    private readonly string _consumerName;
    private readonly string _logName;

    public ReadStreamBlock(ILogger<ReadStreamBlock> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
        _consumerGroupId = "events_consumer_group-" + Guid.NewGuid();
        _consumerName = $"consumer-{_consumerGroupId}";
        _logName = "itemsLog";
    }

    public async Task<TransformBlock<int, IReadOnlyCollection<MyDto>>> BuildAsync(ExecutionDataflowBlockOptions options)
    {
        var redisDb = _connectionMultiplexer.GetDatabase();

        await redisDb.StreamCreateConsumerGroupAsync(_logName, _consumerGroupId, StreamPosition.NewMessages);

        return new(x => TryReadStreamForItemsAsync(), options);
    }

    public async Task<IReadOnlyCollection<MyDto>> TryReadStreamForItemsAsync()
    {
        try
        {
            return await ReadStreamForItemsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured");
            // TODO: Handle exception according to requirements

            return [];
        }
    }

    public async Task<IReadOnlyCollection<MyDto>> ReadStreamForItemsAsync()
    {
        var redisDb = _connectionMultiplexer.GetDatabase();

        var streamEntries = await redisDb.StreamReadGroupAsync(_logName, _consumerGroupId, _consumerName, ">");

        var result = new List<MyDto>(streamEntries.Length);
        var acknowledgeTasks = new List<Task>(streamEntries.Length);

        foreach (var entry in streamEntries)
        {
            result.Add(new MyDto
            {
                Id = entry["id"],
                Name = entry["name"]
            });

            acknowledgeTasks.Add(redisDb.StreamAcknowledgeAsync(_logName, _consumerGroupId, entry.Id));
        }

        await Task.WhenAll(acknowledgeTasks);

        return result;
    }
}