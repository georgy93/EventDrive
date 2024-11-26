namespace EventDrive.API.Swagger;

public record SwaggerSettings
{
    public string JsonRoute { get; init; }

    public string Description { get; init; }

    public string UIEndpoint { get; init; }
}