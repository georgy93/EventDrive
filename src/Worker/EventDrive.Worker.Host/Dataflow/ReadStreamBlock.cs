namespace EventDrive.Worker.Host.Dataflow
{
    using DTOs;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks.Dataflow;

    public class ReadStreamBlock
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        private readonly string _consumerGroupId;
        private readonly string _consumerName;
        private readonly string _logName;

        public ReadStreamBlock(IConnectionMultiplexer connectionMultiplexer)
        {
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

                var info = redisDb.StreamGroupInfo(_logName);
                //var alo = redisDb.delete("itemsLog", info.LastEntry.Id);

                var streamEntries = redisDb.StreamReadGroup(_logName, _consumerGroupId, _consumerName, ">");

                //  var streamEntries = await redisDb.StreamReadAsync("itemsLog", "0-0");
                var result = streamEntries.Select(entry =>
                {
                    redisDb.StreamAcknowledge(_logName, _consumerGroupId, entry.Id);

                    return new MyDTO
                    {
                        Id = entry.Values.GetValue(0).ToString(),
                        Name = entry.Values.GetValue(1).ToString()
                    };
                });

                return result;
            }

            catch (Exception ex)
            {
                return Enumerable.Empty<MyDTO>();
            }
        }
    }
}