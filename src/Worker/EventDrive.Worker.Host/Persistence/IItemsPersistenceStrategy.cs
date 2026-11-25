namespace EventDrive.Worker.Host.Persistence;

using DTOs;

public interface IItemsPersistenceStrategy
{
    Task<int> InsertIgnoringExistingAsync(IReadOnlyCollection<MyDto> items, CancellationToken cancellationToken);
}