namespace EventDrive.Worker.Host.Dataflow;

using DTOs;
using StackExchange.Redis;

public sealed record StreamBatch(IReadOnlyCollection<MyDto> Items, RedisValue[] EntryIds)
{
    public static readonly StreamBatch Empty = new([], []);
}