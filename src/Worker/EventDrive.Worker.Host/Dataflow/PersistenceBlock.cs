namespace EventDrive.Worker.Host.Dataflow
{
    using DTOs;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    public class PersistenceBlock
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ActionBlock<IEnumerable<MyDTO>> Build(ExecutionDataflowBlockOptions options) => new(x => PersistBatchToDatabaseAsync(x), options);

        private async Task PersistBatchToDatabaseAsync(IEnumerable<MyDTO> items)
        {
            var itemsList = items.ToList();

            if (!itemsList.Any())
                return;

            using var sqlConnection = new SqlConnection(_connectionString);

            var dataTable = CreateDataTableFromItems(itemsList);

            await sqlConnection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(sqlConnection)
            {
                BulkCopyTimeout = 20
            };

            // the following 3 lines might not be neccessary
            bulkCopy.DestinationTableName = "Items";
            bulkCopy.ColumnMappings.Add(nameof(MyDTO.Id), "Id");
            bulkCopy.ColumnMappings.Add(nameof(MyDTO.Name), "Name");

            await bulkCopy.WriteToServerAsync(dataTable);
        }

        private static DataTable CreateDataTableFromItems(IEnumerable<MyDTO> items)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Id", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Name", typeof(string)));

            foreach (var item in items)
            {
                dataTable.Rows.Add(new string[] { item.Id, item.Name });
            }

            return dataTable;
        }
    }
}