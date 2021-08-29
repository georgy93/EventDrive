namespace EventDrive.Worker.Host.Dataflow
{
    using DTOs;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    public class PersistenceBlock
    {
        private readonly IConfiguration _configuration;

        public PersistenceBlock(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ActionBlock<IEnumerable<MyDTO>> Build(ExecutionDataflowBlockOptions options) => new(x => PersistBatchToDatabaseAsync(x), options);

        private async Task PersistBatchToDatabaseAsync(IEnumerable<MyDTO> items)
        {
            var itemsList = items.ToList();

            if (!itemsList.Any())
                return;

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var sqlConnection = new SqlConnection(connectionString);

            var dataTable = CreateDataTableFromItems(itemsList);

            await sqlConnection.OpenAsync();
            var transaction = sqlConnection.BeginTransaction();

            using var bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.KeepIdentity, transaction)
            {
                BulkCopyTimeout = 20
            };

            bulkCopy.DestinationTableName = "[Items]";
            bulkCopy.ColumnMappings.Add("Id", "Id");
            bulkCopy.ColumnMappings.Add("Name", "Name");

            try
            {
                await bulkCopy.WriteToServerAsync(dataTable);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
            }
        }

        private static DataTable CreateDataTableFromItems(IEnumerable<MyDTO> items)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("Id", typeof(Guid)));
            table.Columns.Add(new DataColumn("Name", typeof(string)));

            foreach (var item in items)
            {
                table.Rows.Add(item.Id, item.Name);
            }

            return table;
        }
    }
}