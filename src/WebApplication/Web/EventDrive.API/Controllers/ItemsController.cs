namespace EventDrive.API.Controllers
{
    using DTOs.Commands;
    using Infrastructure.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Route("items")]
    public class ItemsController : BaseController
    {
        private readonly IRedisService _redisService;

        public ItemsController(IRedisService redisService)
        {
            _redisService = redisService;
        }

        [HttpPost("addItemsToRedis")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task AddItemsToRedisAsync(AddItemsCommand addItemsCommand)
        {
            await _redisService.WriteToStreamAsync(addItemsCommand.Items);
        }

        [HttpPost("itemsAdded")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task NotifyItemsAddedAsync()
        {
            await Task.Delay(100);
        }
    }
}