namespace EventDrive.Worker.Host.Persistence;

public enum DatabaseProvider
{
    MsSql,
    Postgres
}

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.MsSql;
}