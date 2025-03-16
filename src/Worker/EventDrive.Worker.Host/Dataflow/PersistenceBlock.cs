namespace EventDrive.Worker.Host.Dataflow;

using DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks.Dataflow;

public class PersistenceBlock
{
    private readonly ILogger<PersistenceBlock> _logger;
    private readonly IConfiguration _configuration;

    public PersistenceBlock(ILogger<PersistenceBlock> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public ActionBlock<IReadOnlyCollection<MyDto>> Build(ExecutionDataflowBlockOptions options) => new(x => TryPersistBatchToDatabaseAsync(x), options);

    private async Task TryPersistBatchToDatabaseAsync(IReadOnlyCollection<MyDto> items)
    {
        try
        {
            await PersistBatchToDatabaseAsync(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An Error Occured");
            // TODO: Possibly retry the operation so that the data is not lost
        }
    }

    private async Task PersistBatchToDatabaseAsync(IReadOnlyCollection<MyDto> items)
    {
        if (!items.Any())
            return;

        using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var dataTable = CreateDataTableFromItems(items);

        await sqlConnection.OpenAsync();
        await BulkInsertAsync(sqlConnection, dataTable);
    }

    private static DataTable CreateDataTableFromItems(IEnumerable<MyDto> items)
    {
        var table = new DataTable();
        table.Columns.Add(new DataColumn("ItemId", typeof(string)));
        table.Columns.Add(new DataColumn("ItemName", typeof(string)));

        foreach (var item in items)
        {
            table.Rows.Add(item.Id, item.Name);
        }

        return table;
    }

    private static async Task BulkInsertAsync(SqlConnection sqlConnection, DataTable dataTable)
    {
        using var bulkCopy = new SqlBulkCopy(sqlConnection)
        {
            BulkCopyTimeout = 20
        };

        bulkCopy.DestinationTableName = "[dbo].[Items]";
        bulkCopy.ColumnMappings.Add("ItemId", "ItemId");
        bulkCopy.ColumnMappings.Add("ItemName", "ItemName");

        await bulkCopy.WriteToServerAsync(dataTable);
    }
}