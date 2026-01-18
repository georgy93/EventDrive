namespace EventDrive.Infrastructure.Services.Concrete;

using Abstract;
using DTOs;
using StackExchange.Redis;

internal class RedisService : IEventStreamService
{
    private const string StreamKey = "itemsLog";

    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task WriteToStreamAsync(IEnumerable<MyDto> items, CancellationToken cancellationToken)
    {
        var redisDb = _connectionMultiplexer.GetDatabase();

        foreach (var item in items)
        {
            await StreamDataAsync(redisDb, item);
        }
    }

    private static async Task StreamDataAsync(IDatabase redisDb, MyDto item)
    {
        var nameValuePair = new NameValueEntry[]
        {
            new("id", item.Id),
            new("name", item.Name)
        };

        await redisDb.StreamAddAsync(StreamKey, nameValuePair);
    }
}