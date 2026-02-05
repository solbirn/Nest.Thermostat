using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Options;
using Nest.Thermostat.Api.Configuration;
using Nest.Thermostat.Api.Infrastructure;
using Nest.Thermostat.Api.Models;

namespace Nest.Thermostat.Api.Services;

/// <summary>
/// Service for proxying requests to the upstream Nest API and logging traffic.
/// </summary>
public class NestProxyService
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<NestProxySettings> _settingsMonitor;
    private readonly FileLoggingService _loggingService;
    private readonly ILogger<NestProxyService> _logger;

    private NestProxySettings Settings => _settingsMonitor.CurrentValue;

    public NestProxyService(
        HttpClient httpClient,
        IOptionsMonitor<NestProxySettings> settingsMonitor,
        FileLoggingService loggingService,
        ILogger<NestProxyService> logger)
    {
        _httpClient = httpClient;
        _settingsMonitor = settingsMonitor;
        _loggingService = loggingService;
        _logger = logger;
    }

    private static string GenerateCorrelationId(string method, string path)
    {
        var sanitizedPath = path
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace(".", "-")
            .Replace("?", "_")
            .Replace("&", "_")
            .Replace("=", "_")
            .Replace(" ", "_")
            .Replace(":", "")
            .Trim('-', '_');

        if (sanitizedPath.Length > 30)
            sanitizedPath = sanitizedPath[..30];

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8];

        return $"{method}_{sanitizedPath}_{timestamp}_{guid}";
    }

    /// <summary>
    /// Proxy a request to the upstream Nest API
    /// </summary>
    public async Task<NestProxyResponse> ProxyRequestAsync(
        NestProxyRequest request,
        CancellationToken cancellationToken = default)
    {
        var correlationId = GenerateCorrelationId(request.Method, request.Path);
        var startTime = DateTimeOffset.UtcNow;

        _logger.LogInformation("[NestProxy] {CorrelationId} Proxying {Method} {Path}?{QueryString} to upstream",
            correlationId, request.Method, request.Path, request.QueryString ?? "(none)");

        NestProxyResponse response;

        try
        {
            var upstreamUrl = BuildUpstreamUrl(request.Path, request.QueryString);
            var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), upstreamUrl);

            CopyRequestHeaders(request, httpRequest);

            if (request.Body is not null)
            {
                httpRequest.Content = new StringContent(
                    request.Body,
                    Encoding.UTF8,
                    request.ContentType ?? "application/json");
            }

            var upstreamResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseBody = await ReadAndDecodeResponseAsync(upstreamResponse, cancellationToken);

            response = new NestProxyResponse
            {
                StatusCode = (int)upstreamResponse.StatusCode,
                Headers = ExtractResponseHeaders(upstreamResponse),
                Body = responseBody,
                ContentType = upstreamResponse.Content.Headers.ContentType?.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NestProxy] {CorrelationId} Error proxying request to upstream", correlationId);

            response = new NestProxyResponse
            {
                StatusCode = 502,
                Body = JsonSerializer.Serialize(new { error = "Upstream unavailable", message = ex.Message }),
                ContentType = "application/json",
                Error = ex.Message
            };
        }

        var endTime = DateTimeOffset.UtcNow;
        var duration = endTime - startTime;

        // Log traffic if enabled
        if (Settings.IsLoggingEnabled && Settings.LogToFile)
        {
            _ = _loggingService.LogProxyTrafficAsync(
                correlationId,
                request.Method,
                request.Path,
                Settings.LogBody ? request.Body : null,
                response.StatusCode,
                Settings.LogBody ? response.Body : null,
                request.Headers,
                response.Headers,
                cancellationToken);
        }

        _logger.LogInformation("[NestProxy] {CorrelationId} Completed {Method} {Path} -> {StatusCode} in {Duration}ms",
            correlationId, request.Method, request.Path, response.StatusCode, duration.TotalMilliseconds);

        return response;
    }

    /// <summary>
    /// Initiate a streaming proxy request
    /// </summary>
    public async Task<NestStreamingProxyResponse> InitiateStreamingRequestAsync(
        NestProxyRequest request,
        CancellationToken cancellationToken = default)
    {
        var correlationId = GenerateCorrelationId(request.Method, request.Path);

        _logger.LogInformation("[NestProxy] {CorrelationId} Initiating streaming {Method} {Path} to upstream",
            correlationId, request.Method, request.Path);

        var upstreamUrl = BuildUpstreamUrl(request.Path, request.QueryString);
        var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), upstreamUrl);

        CopyRequestHeaders(request, httpRequest);

        if (request.Body is not null)
        {
            httpRequest.Content = new StringContent(
                request.Body,
                Encoding.UTF8,
                request.ContentType ?? "application/json");
        }

        try
        {
            var upstreamResponse = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            _logger.LogInformation("[NestProxy] {CorrelationId} Streaming response initiated: {StatusCode}",
                correlationId, (int)upstreamResponse.StatusCode);

            var contentStream = await upstreamResponse.Content.ReadAsStreamAsync(cancellationToken);
            var contentEncoding = upstreamResponse.Content.Headers.ContentEncoding.FirstOrDefault()?.ToLowerInvariant();

            Stream decompressedStream = contentEncoding switch
            {
                "gzip" => new GZipStream(contentStream, CompressionMode.Decompress),
                "deflate" => new DeflateStream(contentStream, CompressionMode.Decompress),
                "br" => new BrotliStream(contentStream, CompressionMode.Decompress),
                _ => contentStream
            };

            return new NestStreamingProxyResponse
            {
                StatusCode = (int)upstreamResponse.StatusCode,
                Headers = ExtractResponseHeaders(upstreamResponse),
                ContentStream = decompressedStream
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NestProxy] {CorrelationId} Error initiating streaming request", correlationId);

            return new NestStreamingProxyResponse
            {
                StatusCode = 502,
                Headers = new Dictionary<string, string>(),
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Stream the response body
    /// </summary>
    public async IAsyncEnumerable<string> StreamResponseAsync(
        NestStreamingProxyResponse streamingResponse,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (streamingResponse.ContentStream is null)
        {
            var errorJson = JsonSerializer.Serialize(new { error = "upstream_unavailable", status = streamingResponse.StatusCode, message = streamingResponse.Error });
            yield return errorJson;
            yield break;
        }

        using var reader = new StreamReader(streamingResponse.ContentStream, Encoding.UTF8);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            yield return line;
        }
    }

    private string BuildUpstreamUrl(string path, string? queryString)
    {
        var baseUrl = Settings.UpstreamBaseUrl.TrimEnd('/');
        var fullPath = path.StartsWith('/') ? path : $"/{path}";

        if (!string.IsNullOrEmpty(queryString))
        {
            return $"{baseUrl}{fullPath}?{queryString}";
        }

        return $"{baseUrl}{fullPath}";
    }

    private static void CopyRequestHeaders(NestProxyRequest request, HttpRequestMessage httpRequest)
    {
        if (request.Headers is null) return;

        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private static Dictionary<string, string> ExtractResponseHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>();

        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        foreach (var header in response.Content.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        return headers;
    }

    private async Task<string> ReadAndDecodeResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var rawBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentEncoding = response.Content.Headers.ContentEncoding.FirstOrDefault()?.ToLowerInvariant();

        byte[] decompressedBytes = contentEncoding switch
        {
            "gzip" => await DecompressAsync(rawBytes, CompressionMode.Decompress, stream => new GZipStream(stream, CompressionMode.Decompress), cancellationToken),
            "deflate" => await DecompressAsync(rawBytes, CompressionMode.Decompress, stream => new DeflateStream(stream, CompressionMode.Decompress), cancellationToken),
            "br" => await DecompressAsync(rawBytes, CompressionMode.Decompress, stream => new BrotliStream(stream, CompressionMode.Decompress), cancellationToken),
            _ => rawBytes
        };

        return ConvertBytesToText(decompressedBytes);
    }

    private static async Task<byte[]> DecompressAsync(
        byte[] compressedData,
        CompressionMode mode,
        Func<Stream, Stream> createDecompressionStream,
        CancellationToken cancellationToken)
    {
        await using var compressedStream = new MemoryStream(compressedData);
        await using var decompressionStream = createDecompressionStream(compressedStream);
        await using var resultStream = new MemoryStream();
        await decompressionStream.CopyToAsync(resultStream, cancellationToken);
        return resultStream.ToArray();
    }

    private static string ConvertBytesToText(byte[] bytes)
    {
        if (bytes.Length == 0)
            return string.Empty;

        try
        {
            var text = Encoding.UTF8.GetString(bytes);
            if (!text.Contains('\uFFFD'))
            {
                return text;
            }
        }
        catch
        {
            // Fall through
        }

        try
        {
            return Encoding.Latin1.GetString(bytes);
        }
        catch
        {
            return $"[BINARY DATA: {bytes.Length} bytes]";
        }
    }
}
