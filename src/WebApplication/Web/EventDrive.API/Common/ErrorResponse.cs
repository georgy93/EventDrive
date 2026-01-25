namespace EventDrive.API.Common;

using System.Text.Json.Serialization;

public record ErrorResponse
{
    public string ErrorCode { get; init; }

    public string Description { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Exception Exception { get; init; }
}