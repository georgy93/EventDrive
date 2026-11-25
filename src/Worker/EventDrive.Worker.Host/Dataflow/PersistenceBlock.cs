namespace EventDrive.Worker.Host.Dataflow;

using Persistence;
using System.Threading.Tasks.Dataflow;

public class PersistenceBlock
{
    private readonly ILogger<PersistenceBlock> _logger;
    private readonly IItemsPersistenceStrategy _persistenceStrategy;

    public PersistenceBlock(ILogger<PersistenceBlock> logger, IItemsPersistenceStrategy persistenceStrategy)
    {
        _logger = logger;
        _persistenceStrategy = persistenceStrategy;
    }

    public TransformManyBlock<StreamBatch, StreamBatch> Build(ExecutionDataflowBlockOptions options) => new(TryPersistBatchToDatabaseAsync, options);

    private async Task<IEnumerable<StreamBatch>> TryPersistBatchToDatabaseAsync(StreamBatch batch)
    {
        if (batch.EntryIds.Length == 0)
            return [];

        try
        {
            var insertedCount = await _persistenceStrategy.InsertIgnoringExistingAsync(batch.Items, CancellationToken.None);

            if (insertedCount < batch.Items.Count)
                _logger.LogInformation("Skipped {Count} items whose ItemId already exists", batch.Items.Count - insertedCount);

            return [batch];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist a batch of {Count} items", batch.Items.Count);
            // TODO: Possibly retry the operation so that the data is not lost

            return [];
        }
    }
}
