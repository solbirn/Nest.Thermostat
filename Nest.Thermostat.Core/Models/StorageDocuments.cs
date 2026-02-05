namespace Nest.Thermostat.Core.Models;

/// <summary>
/// Base class for all Nest Thermostat storage documents with type discriminator
/// </summary>
public abstract class StorageDocument
{
    /// <summary>
    /// Unique document ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Device serial number (partition key in original Cosmos design)
    /// </summary>
    [JsonPropertyName("serial")]
    public string Serial { get; set; } = default!;

    /// <summary>
    /// Type discriminator for querying specific document types
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>
    /// Timestamp when document was last updated (Unix ms)
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public long UpdatedAt { get; set; }
}

/// <summary>
/// Device state object - represents a single object in the Nest protocol
/// (device.SERIAL, shared.SERIAL, user.USERID, etc.)
/// </summary>
public class DeviceStateDocument : StorageDocument
{
    public override string Type => "DeviceState";

    /// <summary>
    /// Object key (e.g., "device.ABC123", "shared.ABC123")
    /// </summary>
    [JsonPropertyName("objectKey")]
    public string ObjectKey { get; set; } = default!;

    /// <summary>
    /// Object revision number for optimistic concurrency
    /// </summary>
    [JsonPropertyName("objectRevision")]
    public long ObjectRevision { get; set; }

    /// <summary>
    /// Object timestamp from device
    /// </summary>
    [JsonPropertyName("objectTimestamp")]
    public long ObjectTimestamp { get; set; }

    /// <summary>
    /// Arbitrary JSON value containing device state
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// Get Value as a JsonElement (parsing from raw storage)
    /// </summary>
    public JsonElement? GetValueAsJsonElement()
    {
        if (Value is null)
            return null;

        if (Value is JsonElement element)
            return element.ValueKind != JsonValueKind.Undefined ? element.Clone() : null;

        try
        {
            var json = JsonSerializer.Serialize(Value);
            return JsonDocument.Parse(json).RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Weather cache entry
/// </summary>
public class WeatherCacheDocument : StorageDocument
{
    public override string Type => "WeatherCache";

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = default!;

    [JsonPropertyName("country")]
    public string Country { get; set; } = default!;

    [JsonPropertyName("fetchedAt")]
    public long FetchedAt { get; set; }

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}

/// <summary>
/// Entry key for device pairing (7-character code)
/// </summary>
public class EntryKeyDocument : StorageDocument
{
    public override string Type => "EntryKey";

    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("expiresAt")]
    public long ExpiresAt { get; set; }

    [JsonPropertyName("claimedBy")]
    public string? ClaimedBy { get; set; }

    [JsonPropertyName("claimedAt")]
    public long? ClaimedAt { get; set; }
}

/// <summary>
/// Device ownership record - links a device to a user account
/// </summary>
public class DeviceOwnershipDocument : StorageDocument
{
    public override string Type => "DeviceOwnership";

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = default!;

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("userEmail")]
    public string? UserEmail { get; set; }

    [JsonPropertyName("claimedAt")]
    public long ClaimedAt { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("entryKey")]
    public string? EntryKey { get; set; }
}
