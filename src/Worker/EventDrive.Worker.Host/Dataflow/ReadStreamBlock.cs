namespace EventDrive.Worker.Host.Dataflow;

using DTOs;
using DTOs.Streams;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Threading.Tasks.Dataflow;

public class ReadStreamBlock
{
    private readonly ILogger<ReadStreamBlock> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly RedisStreamOptions _options;

    public ReadStreamBlock(ILogger<ReadStreamBlock> logger, IConnectionMultiplexer connectionMultiplexer, IOptions<RedisStreamOptions> options)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
        _options = options.Value;
    }

    /// <summary>
    /// Builds a block that, for every trigger it receives, drains the stream in batches of at most
    /// <see cref="RedisStreamOptions.BatchSize"/>: first entries left unacknowledged for too long, then new entries.
    /// Entries are not acknowledged here; send persisted batches to <see cref="BuildAcknowledgeBlock"/>.
    /// </summary>
    public async Task<TransformManyBlock<int, StreamBatch>> BuildAsync(ExecutionDataflowBlockOptions options)
    {
        await EnsureConsumerGroupExistsAsync();

        return new(_ => ReadBatchesAsync(), options);
    }

    public ActionBlock<StreamBatch> BuildAcknowledgeBlock(ExecutionDataflowBlockOptions options) => new(TryAcknowledgeAsync, options);

    private async Task EnsureConsumerGroupExistsAsync()
    {
        try
        {
            await Database.StreamCreateConsumerGroupAsync(ItemsStream.Name, _options.ConsumerGroup, StreamPosition.Beginning, createStream: true);
        }
        catch (RedisServerException ex) when (ex.Message.StartsWith("BUSYGROUP", StringComparison.Ordinal))
        {
            // The group already exists, created by an earlier run or another worker instance.
        }
    }

    private async IAsyncEnumerable<StreamBatch> ReadBatchesAsync()
    {
        var claimCursor = (RedisValue)"0-0";
        var claimingDone = false;

        while (true)
        {
            StreamBatch batch;

            try
            {
                if (!claimingDone)
                {
                    var claimed = await ClaimStaleEntriesAsync(claimCursor);
                    claimCursor = claimed.NextStartId;
                    claimingDone = claimCursor == "0-0";
                    batch = ToBatch(await DeadLetterExhaustedEntriesAsync(claimed.ClaimedEntries));

                    if (!batch.EntryIds.Any())
                        continue; // Nothing claimed in this range; claim the next range or move on to new entries.
                }
                else
                {
                    var newEntries = await Database.StreamReadGroupAsync(ItemsStream.Name, _options.ConsumerGroup, _options.ConsumerName, StreamPosition.NewMessages, _options.BatchSize);

                    batch = ToBatch(newEntries);

                    if (!batch.EntryIds.Any())
                        yield break; // The stream is drained.
                }
            }
            catch (Exception ex)
            {
                // Unacknowledged entries stay pending in Redis and are reclaimed by a later read.
                _logger.LogError(ex, "Failed to read stream {StreamName} for consumer group {ConsumerGroup}", ItemsStream.Name, _options.ConsumerGroup);

                yield break;
            }

            yield return batch;
        }
    }

    private Task<StreamAutoClaimResult> ClaimStaleEntriesAsync(RedisValue startAtId) => Database.StreamAutoClaimAsync(
        ItemsStream.Name,
        _options.ConsumerGroup,
        _options.ConsumerName,
        (long)_options.ClaimIdleTime.TotalMilliseconds,
        startAtId,
        _options.BatchSize);

    /// <summary>
    /// Moves claimed entries that were already delivered more than <see cref="RedisStreamOptions.MaxDeliveryAttempts"/>
    /// times to the dead-letter stream and acknowledges them, so a batch that always fails is not retried forever.
    /// Returns the entries that should still be processed.
    /// </summary>
    private async Task<StreamEntry[]> DeadLetterExhaustedEntriesAsync(StreamEntry[] claimed)
    {
        if (!claimed.Any())
            return claimed;

        // Claimed entries come back in ID order, so the first and last IDs bound the range.
        var pending = await Database.StreamPendingMessagesAsync(
            ItemsStream.Name,
            _options.ConsumerGroup,
            claimed.Length,
            _options.ConsumerName,
            minId: claimed[0].Id,
            maxId: claimed[^1].Id);

        var exhaustedIds = pending
            .Where(p => p.DeliveryCount > _options.MaxDeliveryAttempts)
            .Select(p => p.MessageId)
            .ToHashSet();

        if (!exhaustedIds.Any())
            return claimed;

        var exhausted = claimed.Where(e => exhaustedIds.Contains(e.Id)).ToArray();

        foreach (var entry in exhausted)
        {
            NameValueEntry[] values =
            [
                .. entry.Values,
                new("originalEntryId", entry.Id),
                new("consumerGroup", _options.ConsumerGroup),
                new("deadLetteredAt", DateTimeOffset.UtcNow.ToString("O"))
            ];

            await Database.StreamAddAsync(ItemsStream.DeadLetterName, values);
        }

        // Acknowledge only after they are safely in the dead-letter stream.
        await Database.StreamAcknowledgeAsync(ItemsStream.Name, _options.ConsumerGroup, exhausted.Select(e => e.Id).ToArray());

        _logger.LogWarning("Moved {Count} entries to {DeadLetterStream} after more than {MaxDeliveryAttempts} delivery attempts: {EntryIds}",
            exhausted.Length, ItemsStream.DeadLetterName, _options.MaxDeliveryAttempts, string.Join(", ", exhausted.Select(e => e.Id)));

        return claimed.Where(e => !exhaustedIds.Contains(e.Id)).ToArray();
    }

    private StreamBatch ToBatch(StreamEntry[] entries)
    {
        var items = new List<StreamItem>(entries.Length);
        var entryIds = new RedisValue[entries.Length];

        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            entryIds[i] = entry.Id;

            var id = entry[ItemsStream.Fields.Id];
            var name = entry[ItemsStream.Fields.Name];

            if (id.IsNullOrEmpty || name.IsNull)
            {
                _logger.LogWarning("Skipping stream entry {EntryId}: it has no 'id' or 'name' field", entry.Id);
                continue;
            }

            items.Add(new StreamItem(entry.Id, new MyDto { Id = id, Name = name! }));
        }

        return new StreamBatch(items, entryIds);
    }

    private async Task TryAcknowledgeAsync(StreamBatch batch)
    {
        try
        {
            await Database.StreamAcknowledgeAsync(ItemsStream.Name, _options.ConsumerGroup, batch.EntryIds);
        }
        catch (Exception ex)
        {
            // The entries were persisted but stay pending; they will be reclaimed, skipped by the insert as
            // already persisted, and acknowledged then.
            _logger.LogError(ex, "Failed to acknowledge {Count} entries in stream {StreamName}", batch.EntryIds.Length, ItemsStream.Name);
        }
    }

    private IDatabase Database => _connectionMultiplexer.GetDatabase();
}
