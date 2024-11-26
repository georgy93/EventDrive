namespace EventDrive.DTOs.Commands;

using System.Collections.Generic;

public record AddItemsCommand
{
    public AddItemsCommand() { }

    public AddItemsCommand(IEnumerable<MyDto> items)
    {
        Items = items;
    }

    public IEnumerable<MyDto> Items { get; init; } = [];
}