namespace EventDrive.Worker.Host.Dataflow
{
    using DTOs;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public TransformBlock<int, IEnumerable<MyDTO>> Build(ExecutionDataflowBlockOptions options) => new(x => ReadStreamForItems(), options);

        public IEnumerable<MyDTO> ReadStreamForItems()
        {
            try
            {
                var redisDb = _connectionMultiplexer.GetDatabase();
                var streamEntries = redisDb.StreamReadGroup(_logName, _consumerGroupId, _consumerName, ">");

                var result = new List<MyDTO>();

                foreach (var entry in streamEntries)
                {
                    result.Add(new MyDTO
                    {
                        Id = entry["id"],
                        Name = entry["name"]
                    });

                    redisDb.StreamAcknowledge(_logName, _consumerGroupId, entry.Id);
                }

                return result;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured");
                return Enumerable.Empty<MyDTO>();
            }
        }
    }
}