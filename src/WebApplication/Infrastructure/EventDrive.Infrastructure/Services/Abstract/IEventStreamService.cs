namespace EventDrive.Infrastructure.Services.Abstract
{
    using DTOs;
    using System.Collections.Generic;

    public interface IEventStreamService
    {
        void WriteToStream(IEnumerable<MyDTO> items);
    }
}