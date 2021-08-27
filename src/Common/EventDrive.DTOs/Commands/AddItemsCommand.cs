namespace EventDrive.DTOs.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    public record AddItemsCommand
    {
        public AddItemsCommand() { }

        public AddItemsCommand(IEnumerable<MyDTO> items) { Items = items; }

        public IEnumerable<MyDTO> Items { get; init; } = Enumerable.Empty<MyDTO>();
    }
}