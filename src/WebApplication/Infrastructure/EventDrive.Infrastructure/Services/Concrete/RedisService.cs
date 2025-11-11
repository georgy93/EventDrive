namespace EventDrive.Infrastructure.Services.Concrete;

using Abstract;
using DTOs;
using StackExchange.Redis;

internal class RedisService : IEventStreamService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task WriteToStreamAsync(IEnumerable<MyDto> items)
    {
        var redisDb = _connectionMultiplexer.GetDatabase();

        foreach (var item in items)
        {
            var key = "itemsLog";
            var nameValuePair = new NameValueEntry[]
            {
                new("id", item.Id),
                new("name", item.Name)
            };

            await redisDb.StreamAddAsync(key, nameValuePair);
        }
    }
}