namespace EventDrive.Infrastructure.Services.Abstract;

using DTOs.IntegrationEvents;

public interface IIntegrationEventPublisherService
{
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
}