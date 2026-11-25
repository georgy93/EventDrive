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

    public async Task<TransformBlock<int, StreamBatch>> BuildAsync(ExecutionDataflowBlockOptions options)
    {
        var redisDb = _connectionMultiplexer.GetDatabase();

        await redisDb.StreamCreateConsumerGroupAsync(_logName, _consumerGroupId, StreamPosition.NewMessages);

        return new(x => TryReadStreamForItemsAsync(), options);
    }
    
    public ActionBlock<StreamBatch> BuildAcknowledgeBlock(ExecutionDataflowBlockOptions options) => new(TryAcknowledgeAsync, options);

    private async Task<StreamBatch> TryReadStreamForItemsAsync()
    {
        try
        {
            return await ReadStreamForItemsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured");
            // TODO: Handle exception according to requirements

            return StreamBatch.Empty;
        }
    }

    private async Task<StreamBatch> ReadStreamForItemsAsync()
    {
        var redisDb = _connectionMultiplexer.GetDatabase();

        var streamEntries = await redisDb.StreamReadGroupAsync(_logName, _consumerGroupId, _consumerName, ">");

        var items = new List<MyDto>(streamEntries.Length);
        var entryIds = new RedisValue[streamEntries.Length];

        for (var i = 0; i < streamEntries.Length; i++)
        {
            var entry = streamEntries[i];

            items.Add(new MyDto
            {
                Id = entry["id"],
                Name = entry["name"]
            });

            entryIds[i] = entry.Id;
        }

        return new StreamBatch(items, entryIds);
    }

    private async Task TryAcknowledgeAsync(StreamBatch batch)
    {
        if (!batch.EntryIds.Any())
            return;

        try
        {
            var redisDb = _connectionMultiplexer.GetDatabase();

            await redisDb.StreamAcknowledgeAsync(_logName, _consumerGroupId, batch.EntryIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge {Count} entries in stream {StreamName}", batch.EntryIds.Length, _logName);
        }
    }
}
