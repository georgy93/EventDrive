namespace EventDrive.API.Behavior.Settings;

public record ErrorHandlingSettings
{
    public const string SectionName = "ErrorHandlingSettings";

    public bool ShowDetails { get; init; }
}