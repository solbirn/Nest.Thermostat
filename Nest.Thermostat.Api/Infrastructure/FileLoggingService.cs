namespace Nest.Thermostat.Api.Infrastructure;

/// <summary>
/// File-based logging service for proxy traffic logging.
/// Replaces Azure blob-based streaming in the original OwlX implementation.
/// </summary>
public class FileLoggingService
{
    private readonly string _basePath;
    private readonly ILogger<FileLoggingService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileLoggingService(IConfiguration configuration, ILogger<FileLoggingService> logger)
    {
        _basePath = configuration["Logging:FilePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "logs");
        _logger = logger;
        Directory.CreateDirectory(_basePath);
    }

    /// <summary>
    /// Log proxy traffic to file.
    /// </summary>
    public async Task LogProxyTrafficAsync(
        string correlationId,
        string method,
        string path,
        string? requestBody,
        int statusCode,
        string? responseBody,
        Dictionary<string, string>? requestHeaders = null,
        Dictionary<string, string>? responseHeaders = null,
        CancellationToken ct = default)
    {
        var logEntry = new
        {
            correlationId,
            timestamp = DateTimeOffset.UtcNow,
            request = new
            {
                method,
                path,
                headers = requestHeaders,
                body = requestBody
            },
            response = new
            {
                statusCode,
                headers = responseHeaders,
                body = responseBody
            }
        };

        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var logDir = Path.Combine(_basePath, "proxy", date);
        Directory.CreateDirectory(logDir);

        var fileName = $"{SanitizeFileName(method)}_{SanitizeFileName(path)}_{DateTime.UtcNow:HHmmss}_{correlationId[..8]}.json";
        var filePath = Path.Combine(logDir, fileName);

        await _lock.WaitAsync(ct);
        try
        {
            var json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, ct);
            _logger.LogDebug("Logged proxy traffic to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log proxy traffic to {FilePath}", filePath);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Log a general event to file.
    /// </summary>
    public async Task LogEventAsync(string category, string eventId, object data, CancellationToken ct = default)
    {
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var logDir = Path.Combine(_basePath, category, date);
        Directory.CreateDirectory(logDir);

        var fileName = $"{eventId}_{DateTime.UtcNow:HHmmss}.json";
        var filePath = Path.Combine(logDir, fileName);

        await _lock.WaitAsync(ct);
        try
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    private static string SanitizeFileName(string input)
    {
        var sanitized = input.Replace("/", "-").Replace("\\", "-").Replace(":", "-");
        return string.Join("_", sanitized.Split(Path.GetInvalidFileNameChars()));
    }
}
