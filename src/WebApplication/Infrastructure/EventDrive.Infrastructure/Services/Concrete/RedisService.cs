namespace EventDrive.Infrastructure.Services.Concrete;

using Abstract;
using DTOs;
using DTOs.Streams;
using StackExchange.Redis;

internal class RedisService : IEventStreamService
{
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
            new(ItemsStream.Fields.Id, item.Id),
            new(ItemsStream.Fields.Name, item.Name)
        };

        await redisDb.StreamAddAsync(ItemsStream.Name, nameValuePair);
    }
}