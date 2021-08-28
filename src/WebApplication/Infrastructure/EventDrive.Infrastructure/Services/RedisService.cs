namespace EventDrive.Infrastructure.Services
{
    using DTOs;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IEventStreamService
    {
        Task WriteToStreamAsync(IEnumerable<MyDTO> myDTOs);
    }

    internal class RedisService : IEventStreamService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task WriteToStreamAsync(IEnumerable<MyDTO> myDTOs)
        {
            var redisDb = _connectionMultiplexer.GetDatabase();

            myDTOs
                .Select(c => new NameValueEntry[]
                {
                     new NameValueEntry("id", c.Id),
                     new NameValueEntry("name", c.Name)
                })
                .ToList()
                .ForEach(entry => redisDb.StreamAdd("itemsLog", entry));


            var m2 = await redisDb.StreamReadAsync("itemsLog", "0-0");

            var info = redisDb.StreamInfo("itemsLog");

            redisDb.StreamRead("itemsLog", info.LastEntry.Id);

            Console.WriteLine(info.Length);
            Console.WriteLine(info.FirstEntry.Id);
            Console.WriteLine(info.LastEntry.Id);
        }
    }
}