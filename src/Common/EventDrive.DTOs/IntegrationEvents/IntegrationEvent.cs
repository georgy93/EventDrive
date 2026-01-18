namespace EventDrive.DTOs.IntegrationEvents;

using System;
using System.Text.Json.Serialization;

public record IntegrationEvent
{
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime createDate)
    {
        Id = id;
        CreationDate = createDate;
    }

    public Guid Id { get; init; }

    public DateTime CreationDate { get; init; }
}