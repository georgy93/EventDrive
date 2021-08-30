namespace EventDrive.Infrastructure.Services.Abstract
{
    using DTOs.IntegrationEvents;

    public interface IIntegrationEventPublisherService
    {
        void Publish(IntegrationEvent integrationEvent);
    }
}