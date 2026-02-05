namespace Nest.Thermostat.Api.Configuration;

/// <summary>
/// Settings for the Nest proxy service.
/// Supports hot reload via IOptionsMonitor.
/// </summary>
public class NestProxySettings
{
    /// <summary>
    /// Kill switch - when false, all proxy requests return 503 Service Unavailable.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Base URL for the upstream Nest API
    /// </summary>
    public string UpstreamBaseUrl { get; set; } = "https://frontdoor.nest.com";

    /// <summary>
    /// Timeout in seconds for upstream requests
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Whether to enable logging of proxy traffic
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Whether to log proxy traffic to files
    /// </summary>
    public bool LogToFile { get; set; } = true;

    /// <summary>
    /// Whether to log proxy traffic to console
    /// </summary>
    public bool LogToConsole { get; set; } = false;

    /// <summary>
    /// Whether to log request/response bodies
    /// </summary>
    public bool LogBody { get; set; } = false;

    /// <summary>
    /// Maximum body size to log (in characters)
    /// </summary>
    public int MaxLogBodySize { get; set; } = 100000;

    /// <summary>
    /// Comma-separated list of allowed device serial numbers.
    /// If empty, null, or contains "*", all serials are allowed.
    /// </summary>
    public string? AllowedSerials { get; set; }

    private volatile HashSet<string>? _allowedSerialsCache;
    private volatile string? _lastAllowedSerials;
    private readonly object _cacheLock = new();

    /// <summary>
    /// Check if a device serial is allowed to use the proxy.
    /// </summary>
    public bool IsSerialAllowed(string? serial)
    {
        if (string.IsNullOrWhiteSpace(AllowedSerials) || AllowedSerials.Trim() == "*")
            return true;

        if (string.IsNullOrWhiteSpace(serial))
            return false;

        var currentAllowedSerials = AllowedSerials;
        if (_allowedSerialsCache is null || _lastAllowedSerials != currentAllowedSerials)
        {
            lock (_cacheLock)
            {
                if (_allowedSerialsCache is null || _lastAllowedSerials != currentAllowedSerials)
                {
                    _allowedSerialsCache = new HashSet<string>(
                        currentAllowedSerials.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                        StringComparer.OrdinalIgnoreCase);
                    _lastAllowedSerials = currentAllowedSerials;
                }
            }
        }

        return _allowedSerialsCache.Contains(serial);
    }

    /// <summary>
    /// Returns true if any logging is enabled
    /// </summary>
    public bool IsLoggingEnabled => LogToFile || LogToConsole;
}
