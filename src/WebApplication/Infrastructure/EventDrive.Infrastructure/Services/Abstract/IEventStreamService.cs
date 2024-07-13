namespace EventDrive.Infrastructure.Services.Abstract;

using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IEventStreamService
{
    Task WriteToStreamAsync(IEnumerable<MyDTO> items);
}