namespace Nest.Thermostat.Api.Configuration;

/// <summary>
/// API settings for the Nest Thermostat module
/// </summary>
public class NestApiSettings
{
    /// <summary>
    /// Base URL for the Nest API endpoints (used in entry point responses)
    /// </summary>
    public string Origin { get; set; } = "https://localhost:5001";

    /// <summary>
    /// Time-to-live in seconds for entry/pairing keys
    /// </summary>
    public int EntryKeyTtlSeconds { get; set; } = 3600;

    /// <summary>
    /// Cache TTL in milliseconds for weather data
    /// </summary>
    public int WeatherCacheTtlMs { get; set; } = 600000;

    /// <summary>
    /// Server version string returned in entry responses
    /// </summary>
    public string ServerVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Tier name returned in entry responses
    /// </summary>
    public string TierName { get; set; } = "local";
}
