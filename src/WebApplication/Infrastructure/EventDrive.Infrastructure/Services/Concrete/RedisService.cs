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

    public async Task WriteToStreamAsync(IEnumerable<MyDTO> items)
    {
        var redisDb = _connectionMultiplexer.GetDatabase();

        foreach (var item in items)
        {
            await redisDb.StreamAddAsync("itemsLog",
            [
                 new NameValueEntry("id", item.Id),
                 new NameValueEntry("name", item.Name)
            ]);
        }
    }
}