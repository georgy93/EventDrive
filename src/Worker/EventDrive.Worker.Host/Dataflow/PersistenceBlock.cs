namespace EventDrive.Worker.Host.Dataflow;

using DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks.Dataflow;

public class PersistenceBlock
{
    private const string CreateStagingTableSql = """
        CREATE TABLE #ItemsStaging
        (
            [ItemId] [nvarchar] (450) NOT NULL,
            [ItemName] [nvarchar] (MAX) NOT NULL
        );
        """;

    // Inserts only items whose ItemId is not in the table yet ("do nothing on conflict"):
    // - ROW_NUMBER keeps one row when the same ItemId appears more than once in one batch (which one is arbitrary).
    // - UPDLOCK, HOLDLOCK stop a concurrent insert of the same ItemId from slipping in between the check and the insert.
    // - InsertedAt is left out: the column default sets it.
    private const string InsertFromStagingSql = """
        WITH OnePerItemId AS
        (
            SELECT [ItemId], [ItemName],
                   ROW_NUMBER() OVER (PARTITION BY [ItemId] ORDER BY (SELECT NULL)) AS [Occurrence]
            FROM #ItemsStaging
        )
        INSERT INTO [dbo].[Items] ([ItemId], [ItemName])
        SELECT s.[ItemId], s.[ItemName]
        FROM OnePerItemId s
        WHERE s.[Occurrence] = 1
          AND NOT EXISTS (SELECT 1 FROM [dbo].[Items] i WITH (UPDLOCK, HOLDLOCK) WHERE i.[ItemId] = s.[ItemId]);

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
        if (batch.EntryIds.Length == 0)
            return [];

        try
        {
            await PersistBatchToDatabaseAsync(batch.Items, CancellationToken.None);

            return [batch];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist a batch of {Count} items", batch.Items.Count);
            // TODO: Possibly retry the operation so that the data is not lost

            return [];
        }
    }

    private async Task PersistBatchToDatabaseAsync(IReadOnlyCollection<MyDto> items, CancellationToken cancellationToken)
    {
        if (!items.Any())
            return;

        await using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var dataTable = CreateDataTableFromItems(items);

        await sqlConnection.OpenAsync(cancellationToken);

        // Bulk copy into a temp table first: SqlBulkCopy cannot skip conflicting rows itself.
        await ExecuteAsync(sqlConnection, CreateStagingTableSql, cancellationToken);
        await BulkInsertAsync(sqlConnection, dataTable, cancellationToken);
        var inserted = await ExecuteAsync(sqlConnection, InsertFromStagingSql, cancellationToken);

        if (inserted < items.Count)
            _logger.LogInformation("Skipped {Count} items whose ItemId already exists", items.Count - inserted);
    }

    private static DataTable CreateDataTableFromItems(IEnumerable<MyDto> items)
    {
        var table = new DataTable
        {
            Columns =
            {
                new DataColumn("ItemId", typeof(string)),
                new DataColumn("ItemName", typeof(string))
            }
        };

        foreach (var item in items)
        {
            table.Rows.Add(item.Id, item.Name);
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

        bulkCopy.ColumnMappings.Add("ItemId", "ItemId");
        bulkCopy.ColumnMappings.Add("ItemName", "ItemName");

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
    }
}
