namespace EventDrive.API.Controllers
{
    using Common;
    using DTOs.Commands;
    using DTOs.IntegrationEvents;
    using Infrastructure.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Route("api/items")]
    public class ItemsController : BaseController
    {
        [HttpPost("addItemsToRedis")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public Task AddItemsToRedisAsync([FromServices] IEventStreamService eventStreamService, AddItemsCommand addItemsCommand)
        {
            eventStreamService.WriteToStream(addItemsCommand.Items);

            return Task.CompletedTask;
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