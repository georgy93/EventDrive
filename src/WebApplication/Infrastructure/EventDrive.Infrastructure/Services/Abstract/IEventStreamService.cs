namespace EventDrive.Infrastructure.Services.Abstract;

using DTOs;

public interface IEventStreamService
{
    Task WriteToStreamAsync(IEnumerable<MyDTO> items);
}