namespace EventDrive.API.Swagger.Examples
{
    using DTOs;
    using DTOs.Commands;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;

    public class AddItemsCommandExample : IExamplesProvider<AddItemsCommand>
    {
        public AddItemsCommand GetExamples() => new()
        {
            Items = new List<MyDTO>()
            {
                new() { Id = Guid.NewGuid().ToString() },
                new() { Id = Guid.NewGuid().ToString() },
                new() { Id = Guid.NewGuid().ToString() }
            }
        };
    }
}
