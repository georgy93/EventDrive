namespace EventDrive.API.Controllers
{
    using Common;
    using DTOs.Commands;
    using DTOs.IntegrationEvents;
    using Infrastructure.Services.Abstract;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Route("api/items")]
    public class ItemsController : BaseController
    {
        [HttpPost("addItemsToRedis")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task AddItemsToRedisAsync([FromServices] IEventStreamService eventStreamService, AddItemsCommand addItemsCommand)
        {
            await eventStreamService.WriteToStreamAsync(addItemsCommand.Items);
        }

        [HttpPost("itemsAdded")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public Task NotifyItemsAddedAsync([FromServices] IIntegrationEventPublisherService integrationEventPublisherService)
        {
            integrationEventPublisherService.Publish(new ItemsAddedToRedisIntegrationEvent());

            return Task.CompletedTask;
        }
    }
}