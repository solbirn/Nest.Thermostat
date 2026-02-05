namespace Nest.Thermostat.Api.Models;

#region Proxy Models

/// <summary>
/// Proxy request to upstream Nest API
/// </summary>
public class NestProxyRequest
{
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = default!;
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
    public string? ContentType { get; set; }
    public string? DeviceSerial { get; set; }
    public string? QueryString { get; set; }
}

/// <summary>
/// Proxy response from upstream Nest API
/// </summary>
public class NestProxyResponse
{
    public int StatusCode { get; set; }
    public string? Body { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Error { get; set; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
}

/// <summary>
/// Streaming proxy response
/// </summary>
public class NestStreamingProxyResponse
{
    public int StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public Stream? ContentStream { get; set; }
    public string? Error { get; set; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
}

#endregion

#region Transport Models

/// <summary>
/// Transport subscribe request
/// </summary>
public class TransportSubscribeRequest
{
    [JsonPropertyName("session")]
    public string? Session { get; set; }

    [JsonPropertyName("chunked")]
    public bool? Chunked { get; set; }

    [JsonPropertyName("objects")]
    public List<TransportSubscribeObject>? Objects { get; set; }
}

/// <summary>
/// Object in transport subscribe request
/// </summary>
public class TransportSubscribeObject
{
    [JsonPropertyName("object_key")]
    public string ObjectKey { get; set; } = default!;

    [JsonPropertyName("object_revision")]
    public long? ObjectRevision { get; set; }

    [JsonPropertyName("object_timestamp")]
    public long? ObjectTimestamp { get; set; }

    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }
}

/// <summary>
/// Transport object response
/// </summary>
public class TransportObjectResponse
{
    [JsonPropertyName("object_key")]
    [JsonPropertyOrder(0)]
    public string ObjectKey { get; set; } = default!;

    [JsonPropertyName("object_revision")]
    [JsonPropertyOrder(1)]
    public long ObjectRevision { get; set; }

    [JsonPropertyName("object_timestamp")]
    [JsonPropertyOrder(2)]
    public long ObjectTimestamp { get; set; }

    [JsonPropertyName("value")]
    [JsonPropertyOrder(3)]
    public JsonElement? Value { get; set; }
}

/// <summary>
/// Transport object metadata (without value)
/// </summary>
public class TransportObjectMetadata
{
    [JsonPropertyName("object_revision")]
    public long ObjectRevision { get; set; }

    [JsonPropertyName("object_timestamp")]
    public long ObjectTimestamp { get; set; }

    [JsonPropertyName("object_key")]
    public string ObjectKey { get; set; } = default!;
}

/// <summary>
/// Transport GET response
/// </summary>
public class TransportGetResponse
{
    [JsonPropertyName("objects")]
    public List<TransportObjectMetadata> Objects { get; set; } = [];
}

/// <summary>
/// Transport put request
/// </summary>
public class TransportPutRequest
{
    [JsonPropertyName("session")]
    public string? Session { get; set; }

    [JsonPropertyName("objects")]
    public List<TransportPutObject>? Objects { get; set; }
}

/// <summary>
/// Object in transport put request
/// </summary>
public class TransportPutObject
{
    [JsonPropertyName("object_key")]
    public string ObjectKey { get; set; } = default!;

    [JsonPropertyName("op")]
    public string? Op { get; set; }

    [JsonPropertyName("base_object_revision")]
    public long? BaseObjectRevision { get; set; }

    [JsonPropertyName("object_revision")]
    public long? ObjectRevision { get; set; }

    [JsonPropertyName("object_timestamp")]
    public long? ObjectTimestamp { get; set; }

    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }
}

/// <summary>
/// Transport put/subscribe response
/// </summary>
public class TransportResponse
{
    [JsonPropertyName("objects")]
    public List<TransportObjectResponse> Objects { get; set; } = [];
}

#endregion

#region API Response Models

/// <summary>
/// Entry links response for /nest/entry
/// </summary>
public class EntryLinksResponse
{
    [JsonPropertyName("czfe_url")]
    public string CzfeUrl { get; set; } = default!;

    [JsonPropertyName("transport_url")]
    public string TransportUrl { get; set; } = default!;

    [JsonPropertyName("direct_transport_url")]
    public string DirectTransportUrl { get; set; } = default!;

    [JsonPropertyName("passphrase_url")]
    public string PassphraseUrl { get; set; } = default!;

    [JsonPropertyName("ping_url")]
    public string PingUrl { get; set; } = default!;

    [JsonPropertyName("pro_info_url")]
    public string ProInfoUrl { get; set; } = default!;

    [JsonPropertyName("weather_url")]
    public string WeatherUrl { get; set; } = default!;

    [JsonPropertyName("upload_url")]
    public string UploadUrl { get; set; } = default!;

    [JsonPropertyName("software_update_url")]
    public string SoftwareUpdateUrl { get; set; } = default!;

    [JsonPropertyName("server_version")]
    public string ServerVersion { get; set; } = default!;

    [JsonPropertyName("tier_name")]
    public string TierName { get; set; } = default!;
}

/// <summary>
/// Passphrase response
/// </summary>
public class PassphraseResponse
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;

    [JsonPropertyName("expires")]
    public long Expires { get; set; }
}

/// <summary>
/// Health/ping response
/// </summary>
public class HealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

/// <summary>
/// Error response
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = default!;

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

/// <summary>
/// Device list item
/// </summary>
public class DeviceListItem
{
    [JsonPropertyName("serial")]
    public string Serial { get; set; } = default!;

    [JsonPropertyName("objects")]
    public List<string> Objects { get; set; } = [];
}

#endregion

#region Command Models

/// <summary>
/// Base command request
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "action")]
[JsonDerivedType(typeof(TemperatureCommand), "temp")]
[JsonDerivedType(typeof(AwayCommand), "away")]
[JsonDerivedType(typeof(SetCommand), "set")]
[JsonDerivedType(typeof(ScheduleCommand), "schedule")]
public abstract class CommandRequest
{
    [JsonPropertyName("serial")]
    public string Serial { get; set; } = default!;

    [JsonIgnore]
    public abstract string Action { get; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }
}

/// <summary>
/// Temperature command
/// </summary>
[JsonDerivedType(typeof(TemperatureCommand), "temperature")]
public class TemperatureCommand : CommandRequest
{
    [JsonIgnore]
    public override string Action => "temp";

    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("target_temperature_low")]
    public double? TargetTemperatureLow { get; set; }

    [JsonPropertyName("target_temperature_high")]
    public double? TargetTemperatureHigh { get; set; }
}

/// <summary>
/// Away command
/// </summary>
public class AwayCommand : CommandRequest
{
    [JsonIgnore]
    public override string Action => "away";

    [JsonPropertyName("value")]
    public bool Value { get; set; }
}

/// <summary>
/// Set command
/// </summary>
public class SetCommand : CommandRequest
{
    [JsonIgnore]
    public override string Action => "set";

    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }
}

/// <summary>
/// Schedule command
/// </summary>
public class ScheduleCommand : CommandRequest
{
    [JsonIgnore]
    public override string Action => "schedule";

    [JsonPropertyName("setpoint")]
    public ScheduleSetpoint? Setpoint { get; set; }

    [JsonPropertyName("schedule_mode")]
    public string? ScheduleMode { get; set; }

    [JsonPropertyName("days")]
    public Dictionary<string, Dictionary<string, ScheduleEntry>>? Days { get; set; }
}

/// <summary>
/// Schedule setpoint
/// </summary>
public class ScheduleSetpoint
{
    [JsonPropertyName("day")]
    public int Day { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    [JsonPropertyName("time")]
    public int Time { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("entry_type")]
    public string? EntryType { get; set; }
}

/// <summary>
/// Schedule entry
/// </summary>
public class ScheduleEntry
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    [JsonPropertyName("time")]
    public int Time { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "HEAT";

    [JsonPropertyName("entry_type")]
    public string EntryType { get; set; } = "setpoint";

    [JsonPropertyName("touched_at")]
    public long? TouchedAt { get; set; }

    [JsonPropertyName("touched_by")]
    public int? TouchedBy { get; set; }

    [JsonPropertyName("touched_tzo")]
    public int? TouchedTzo { get; set; }
}

/// <summary>
/// Command response
/// </summary>
public class CommandResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    [JsonPropertyName("device")]
    public string Device { get; set; } = default!;

    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("revision")]
    public long Revision { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

#endregion

#region Device Claiming Models

/// <summary>
/// Claim device request
/// </summary>
public class ClaimDeviceRequest
{
    [JsonPropertyName("entry_key")]
    public string EntryKey { get; set; } = default!;

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }
}

/// <summary>
/// Claim device response
/// </summary>
public class ClaimDeviceResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    [JsonPropertyName("serial")]
    public string? Serial { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("claimed_at")]
    public long? ClaimedAt { get; set; }
}

/// <summary>
/// Update device request
/// </summary>
public class UpdateDeviceRequest
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }
}

#endregion
