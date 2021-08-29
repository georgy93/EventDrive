namespace EventDrive.API.Controllers
{
    using Common;
    using DTOs.Commands;
    using DTOs.IntegrationEvents;
    using Infrastructure.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Route("items")]
    public class ItemsController : BaseController
    {
        private readonly IEventStreamService _eventStreamService;

        public ItemsController(IEventStreamService eventStreamService)
        {
            _eventStreamService = eventStreamService;
        }

        [HttpPost("addItemsToRedis")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public Task AddItemsToRedisAsync(AddItemsCommand addItemsCommand)
        {
            _eventStreamService.WriteToStream(addItemsCommand.Items);

            return Task.CompletedTask;
        }

        [HttpPost("itemsAdded")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public Task NotifyItemsAddedAsync([FromServices] IntegrationEventPublisherService integrationEventPublisherService)
        {
            integrationEventPublisherService.Publish(new ItemsAddedToRedisIntegrationEvent());

            return Task.CompletedTask;
        }
    }
}