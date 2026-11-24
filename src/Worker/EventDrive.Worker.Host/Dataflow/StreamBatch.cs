namespace EventDrive.Worker.Host.Dataflow;

using DTOs;
using StackExchange.Redis;

/// <summary>An item read from the Redis stream, with the ID of the stream entry it came from.</summary>
public sealed record StreamItem(string EntryId, MyDto Item);

/// <summary>
/// Items read from the Redis stream, together with the IDs of the stream entries to acknowledge once they are persisted.
/// <see cref="EntryIds"/> can hold more IDs than there are items: malformed entries are skipped but still acknowledged.
/// </summary>
public sealed record StreamBatch(IReadOnlyCollection<StreamItem> Items, RedisValue[] EntryIds);
