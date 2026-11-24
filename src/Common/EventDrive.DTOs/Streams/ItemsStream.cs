namespace EventDrive.DTOs.Streams;

public static class ItemsStream
{
    public const string Name = "itemsLog";

    /// <summary>Entries that failed too many times are moved here for someone to inspect.</summary>
    public const string DeadLetterName = "itemsLog:dead-letter";

    public static class Fields
    {
        public const string Id = "id";
        public const string Name = "name";
    }
}