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
        private readonly IEventStreamService _redisService;

        public ItemsController(IEventStreamService redisService)
        {
            _redisService = redisService;
        }

        [HttpPost("addItemsToRedis")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task AddItemsToRedisAsync(AddItemsCommand addItemsCommand)
        {
            await _redisService.WriteToStreamAsync(addItemsCommand.Items);
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