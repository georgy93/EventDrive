namespace EventDrive.Infrastructure.Services.Concrete
{
    using Abstract;
    using DTOs;
    using StackExchange.Redis;
    using System.Collections.Generic;

    internal class RedisService : IEventStreamService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public void WriteToStream(IEnumerable<MyDTO> items)
        {
            var redisDb = _connectionMultiplexer.GetDatabase();

            foreach (var item in items)
            {
                redisDb.StreamAdd("itemsLog", new NameValueEntry[]
                {
                     new NameValueEntry("id", item.Id),
                     new NameValueEntry("name", item.Name)
                });
            }
        }
    }
}