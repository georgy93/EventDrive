namespace EventDrive.DTOs.IntegrationEvents;

using Newtonsoft.Json;
using System;

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

    [JsonProperty]
    public Guid Id { get; init; }

    [JsonProperty]
    public DateTime CreationDate { get; init; }
}