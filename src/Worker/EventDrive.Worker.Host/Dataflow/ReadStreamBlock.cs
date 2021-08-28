namespace EventDrive.Worker.Host.Dataflow
{
    using DTOs;
    using StackExchange.Redis;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    public class ReadStreamBlock
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public ReadStreamBlock(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public TransformBlock<int, IEnumerable<MyDTO>> Build(ExecutionDataflowBlockOptions options) => new(x => ReadStreamForItemsAsync(), options);

        public async Task<IEnumerable<MyDTO>> ReadStreamForItemsAsync()
        {
            var redisDb = _connectionMultiplexer.GetDatabase();

            var streamEntries = await redisDb.StreamReadAsync("itemsLog", "0-0");

            return streamEntries.Select(entry => new MyDTO { });
        }
    }
}