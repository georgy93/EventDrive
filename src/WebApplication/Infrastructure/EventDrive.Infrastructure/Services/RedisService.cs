namespace EventDrive.Infrastructure.Services
{
    using EventDrive.DTOs;
    using StackExchange.Redis;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRedisService
    {
        Task WriteToStreamAsync(IEnumerable<MyDTO> myDTOs);
    }

    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task WriteToStreamAsync(IEnumerable<MyDTO> myDTOs)
        {
            var redisValues = new RedisValue();

            var redisDb = _connectionMultiplexer.GetDatabase();
            redisDb.StreamAdd("log", null);

            var publisher = _connectionMultiplexer.GetSubscriber();

            await publisher.PublishAsync("log:notify", string.Empty, CommandFlags.DemandMaster);
        }
    }
}