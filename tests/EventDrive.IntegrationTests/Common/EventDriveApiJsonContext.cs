namespace EventDrive.IntegrationTests.Common;

using DTOs.Commands;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Source-generated JSON metadata for the types <see cref="IEventDriveApiClient"/> sends and receives.
/// Refit's generated clients do not fall back to reflection-based JSON, so every body type must be listed here.
/// Web defaults (camelCase) match the JSON Refit produced before.
/// </summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(AddItemsCommand))]
internal partial class EventDriveApiJsonContext : JsonSerializerContext;
