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

        var redisDb = connectionMultiplexer.GetDatabase();
        redisDb.StreamCreateConsumerGroup(_logName, _consumerGroupId, StreamPosition.NewMessages);
    }

    public TransformBlock<int, IEnumerable<MyDto>> Build(ExecutionDataflowBlockOptions options) => new(x => ReadStreamForItemsAsync(), options);

    public async Task<IEnumerable<MyDto>> ReadStreamForItemsAsync()
    {
        try
        {
            var redisDb = _connectionMultiplexer.GetDatabase();
            var streamEntries = await redisDb.StreamReadGroupAsync(_logName, _consumerGroupId, _consumerName, ">");

            var result = new List<MyDto>();

            foreach (var entry in streamEntries)
            {
                result.Add(new MyDto
                {
                    Id = entry["id"],
                    Name = entry["name"]
                });

                await redisDb.StreamAcknowledgeAsync(_logName, _consumerGroupId, entry.Id);
            }

            return result;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured");
            // TODO: Handle exception according to requirements

            return [];
        }
    }
}