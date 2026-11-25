namespace EventDrive.Worker.Host.Persistence;

using DTOs;
using Npgsql;
using NpgsqlTypes;

public sealed class PostgresItemsPersistenceStrategy : IItemsPersistenceStrategy
{
    // Postgres skips conflicting rows itself, so no staging table is needed: the whole batch is sent as two arrays
    // in one statement. ON CONFLICT DO NOTHING also covers an ItemId repeated within the batch (the first one wins).
    // inserted_at is left out: the column default sets it.
    private const string InsertSql = """
        INSERT INTO items (item_id, item_name)
        SELECT * FROM unnest(@itemIds, @itemNames)
        ON CONFLICT (item_id) DO NOTHING;
        """;

    private const int TimeoutSeconds = 20;

    private readonly NpgsqlDataSource _dataSource;

    public PostgresItemsPersistenceStrategy(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<int> InsertIgnoringExistingAsync(IReadOnlyCollection<MyDto> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return 0;

        await using var command = _dataSource.CreateCommand(InsertSql);
        command.CommandTimeout = TimeoutSeconds;

        command.Parameters.Add(new NpgsqlParameter<string[]>("itemIds", NpgsqlDbType.Array | NpgsqlDbType.Text) { TypedValue = items.Select(x => x.Id).ToArray() });
        command.Parameters.Add(new NpgsqlParameter<string[]>("itemNames", NpgsqlDbType.Array | NpgsqlDbType.Text) { TypedValue = items.Select(x => x.Name).ToArray() });

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
