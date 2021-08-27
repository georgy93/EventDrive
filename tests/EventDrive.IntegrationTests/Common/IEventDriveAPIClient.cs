namespace EventDrive.IntegrationTests.Common
{
    using DTOs.Commands;
    using Refit;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEventDriveAPIClient
    {
        //[Get("/someController/someEndpoint")]
        [Post("/items/addItemsToRedis")]
        Task AddItemsToRedisAsync([Body] AddItemsCommand addItemsCommand, CancellationToken cancellationToken = default);

        [Post("/items/itemsAdded")]
        Task NotifyItemsAddedAsync(CancellationToken cancellationToken = default);
    }
}