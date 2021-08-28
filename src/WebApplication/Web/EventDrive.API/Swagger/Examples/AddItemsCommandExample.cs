namespace EventDrive.API.Swagger.Examples
{
    using DTOs;
    using DTOs.Commands;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Linq;

    public class AddItemsCommandExample : IExamplesProvider<AddItemsCommand>
    {
        public AddItemsCommand GetExamples() => new()
        {
            Items = Enumerable
            .Range(0, 3)
            .ToList()
            .Select(x =>
            {
                var guid = Guid.NewGuid().ToString();

                return new MyDTO
                {
                    Id = guid,
                    Name = guid + "lala"
                };
            }).ToList()
        };
    }
}
