namespace EventDrive.Worker.Host.Persistence;

using DTOs;
using Microsoft.Data.SqlClient;
using System.Data;

public sealed class MsSqlItemsPersistenceStrategy : IItemsPersistenceStrategy
{
    private const string CreateStagingTableSql = """
        CREATE TABLE #ItemsStaging
        (
            [ItemId] [nvarchar] (450) NOT NULL,
            [ItemName] [nvarchar] (MAX) NOT NULL
        );
        """;

    // SQL Server has no "ON CONFLICT DO NOTHING", and SqlBulkCopy cannot skip rows, so the batch is bulk copied
    // into a temp table first and inserted from there, leaving out ItemIds that already exist:
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

    private const int TimeoutSeconds = 20;

    private readonly string _connectionString;

    public MsSqlItemsPersistenceStrategy(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> InsertIgnoringExistingAsync(IReadOnlyCollection<MyDto> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return 0;

        await using var sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await ExecuteAsync(sqlConnection, CreateStagingTableSql, cancellationToken);
        await BulkInsertAsync(sqlConnection, CreateDataTableFromItems(items), cancellationToken);

        return await ExecuteAsync(sqlConnection, InsertFromStagingSql, cancellationToken);
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
        await using var command = new SqlCommand(sql, sqlConnection) { CommandTimeout = TimeoutSeconds };

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task BulkInsertAsync(SqlConnection sqlConnection, DataTable dataTable, CancellationToken cancellationToken)
    {
        using var bulkCopy = new SqlBulkCopy(sqlConnection)
        {
            BulkCopyTimeout = TimeoutSeconds,
            DestinationTableName = "#ItemsStaging"
        };

        bulkCopy.ColumnMappings.Add("ItemId", "ItemId");
        bulkCopy.ColumnMappings.Add("ItemName", "ItemName");

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
    }
}
