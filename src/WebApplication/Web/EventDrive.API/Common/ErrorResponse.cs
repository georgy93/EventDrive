namespace EventDrive.API.Common;

using Newtonsoft.Json;

public record ErrorResponse
{
    public string ErrorCode { get; init; }

    public string Description { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Exception Exception { get; init; }
}