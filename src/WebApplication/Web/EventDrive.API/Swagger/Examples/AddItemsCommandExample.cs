namespace EventDrive.API.Swagger.Examples;

using DTOs;
using DTOs.Commands;
using Swashbuckle.AspNetCore.Filters;

public class AddItemsCommandExample : IExamplesProvider<AddItemsCommand>
{
    public AddItemsCommand GetExamples() => new()
    {
        Items = Enumerable
        .Range(0, 3)
        .Select(x =>
        {
            var guid = Guid.NewGuid().ToString();

            return new MyDto
            {
                Id = guid,
                Name = guid + "lala"
            };
        }).ToList()
    };
}