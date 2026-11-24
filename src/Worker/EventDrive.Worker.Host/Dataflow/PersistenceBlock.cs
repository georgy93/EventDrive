namespace EventDrive.Worker.Host.Dataflow;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks.Dataflow;

public class PersistenceBlock
{
    private const string CreateStagingTableSql = """
        CREATE TABLE #ItemsStaging
        (
            [Seq] [int] NOT NULL,
            [StreamEntryId] [varchar] (64) NOT NULL PRIMARY KEY,
            [ItemId] [nvarchar] (MAX) NOT NULL,
            [ItemName] [nvarchar] (MAX) NOT NULL
        );
        """;

    // Inserts only entries that are not in the table yet, so a redelivered batch does not create duplicate rows.
    // The lock hints stop two workers that hold the same entry from both passing the NOT EXISTS check.
    // ORDER BY Seq keeps the order the entries were read in, as before.
    private const string MergeFromStagingSql = """
        INSERT INTO [dbo].[Items] ([ItemId], [ItemName], [StreamEntryId])
        SELECT s.[ItemId], s.[ItemName], s.[StreamEntryId]
        FROM #ItemsStaging s
        WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Items] i WITH (UPDLOCK, HOLDLOCK) WHERE i.[StreamEntryId] = s.[StreamEntryId])
        ORDER BY s.[Seq];

        DROP TABLE #ItemsStaging;
        """;

    private readonly ILogger<PersistenceBlock> _logger;
    private readonly IConfiguration _configuration;

    public PersistenceBlock(ILogger<PersistenceBlock> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public TransformManyBlock<StreamBatch, StreamBatch> Build(ExecutionDataflowBlockOptions options) => new(TryPersistBatchToDatabaseAsync, options);

    private async Task<IEnumerable<StreamBatch>> TryPersistBatchToDatabaseAsync(StreamBatch batch)
    {
        try
        {
            await PersistBatchToDatabaseAsync(batch.Items, CancellationToken.None);

            return [batch];
        }
        catch (Exception ex)
        {
            // Not acknowledged: the entries stay pending in Redis and are retried once they are reclaimed,
            // until they reach the delivery limit and are moved to the dead-letter stream.
            _logger.LogError(ex, "Failed to persist a batch of {Count} items", batch.Items.Count);

            return [];
        }
    }

    private async Task PersistBatchToDatabaseAsync(IReadOnlyCollection<StreamItem> items, CancellationToken cancellationToken)
    {
        if (!items.Any())
            return;

        await using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await sqlConnection.OpenAsync(cancellationToken);

        await ExecuteAsync(sqlConnection, CreateStagingTableSql, cancellationToken);
        await BulkInsertAsync(sqlConnection, CreateDataTableFromItems(items), cancellationToken);
        var inserted = await ExecuteAsync(sqlConnection, MergeFromStagingSql, cancellationToken);

        if (inserted < items.Count)
            _logger.LogInformation("Skipped {Count} items that were already persisted", items.Count - inserted);
    }

    private static DataTable CreateDataTableFromItems(IEnumerable<StreamItem> items)
    {
        var table = new DataTable
        {
            Columns =
            {
                new DataColumn("Seq", typeof(int)),
                new DataColumn("StreamEntryId", typeof(string)),
                new DataColumn("ItemId", typeof(string)),
                new DataColumn("ItemName", typeof(string))
            }
        };

        var seq = 0;

        foreach (var item in items)
        {
            table.Rows.Add(seq++, item.EntryId, item.Item.Id, item.Item.Name);
        }

        return table;
    }

    private static async Task<int> ExecuteAsync(SqlConnection sqlConnection, string sql, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(sql, sqlConnection) { CommandTimeout = 20 };

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task BulkInsertAsync(SqlConnection sqlConnection, DataTable dataTable, CancellationToken cancellationToken)
    {
        using var bulkCopy = new SqlBulkCopy(sqlConnection)
        {
            BulkCopyTimeout = 20,
            DestinationTableName = "#ItemsStaging"
        };

        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
    }
}
