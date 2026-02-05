using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Nest.Thermostat.Api.Configuration;
using Nest.Thermostat.Api.Infrastructure;
using Nest.Thermostat.Api.Models;
using Nest.Thermostat.Api.Services;

namespace Nest.Thermostat.Api.Controllers;

/// <summary>
/// Nest-compatible device API endpoints (anonymous - for thermostat devices)
/// Proxies all requests to upstream Nest API and logs traffic.
/// </summary>
[Route("nest")]
[AllowAnonymous]
public class NestProxyController : ControllerBase
{
    private readonly NestProxyService _proxyService;
    private readonly IOptionsMonitor<NestProxySettings> _proxySettingsMonitor;
    private readonly NestApiSettings _apiSettings;
    private readonly ILogger<NestProxyController> _logger;

    private NestProxySettings ProxySettings => _proxySettingsMonitor.CurrentValue;

    public NestProxyController(
        NestProxyService proxyService,
        IOptionsMonitor<NestProxySettings> proxySettingsMonitor,
        NestApiSettings apiSettings,
        ILogger<NestProxyController> logger)
    {
        _proxyService = proxyService;
        _proxySettingsMonitor = proxySettingsMonitor;
        _apiSettings = apiSettings;
        _logger = logger;
    }

    private IActionResult? CheckProxyAccess(string? serial)
    {
        var settings = ProxySettings;

        if (!settings.Enabled)
        {
            _logger.LogWarning("[NestProxy] Proxy disabled, returning 503 for {Serial}", serial ?? "unknown");
            return StatusCode(503, new ErrorResponse { Error = "Service temporarily unavailable", Code = "PROXY_DISABLED" });
        }

        if (!settings.IsSerialAllowed(serial))
        {
            _logger.LogWarning("[NestProxy] Serial {Serial} not in allowlist, returning 403", serial ?? "unknown");
            return StatusCode(403, new ErrorResponse { Error = "Device not authorized", Code = "SERIAL_NOT_ALLOWED" });
        }

        return null;
    }

    /// <summary>
    /// GET/POST /nest/entry - Returns service discovery URLs
    /// </summary>
    [HttpGet("entry")]
    [HttpPost("entry")]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60, PolicyName = "nest-device")]
    public async Task<IActionResult> GetEntry(CancellationToken cancellationToken)
    {
        var serial = ResolveDeviceSerial();

        if (CheckProxyAccess(serial) is { } accessDenied)
            return accessDenied;

        string? body = null;
        if (Request.Method == "POST")
        {
            body = await ReadRequestBodyAsync();
        }

        _logger.LogInformation("[NestProxy] Entry {Method} request from {Serial}", Request.Method, serial ?? "unknown");

        var origin = _apiSettings.Origin.TrimEnd('/');
        return Ok(new EntryLinksResponse
        {
            CzfeUrl = $"{origin}/nest/transport",
            TransportUrl = $"{origin}/nest/transport",
            DirectTransportUrl = $"{origin}/nest/transport",
            PassphraseUrl = $"{origin}/nest/passphrase",
            PingUrl = $"{origin}/nest/ping",
            ProInfoUrl = $"{origin}/nest/pro_info",
            WeatherUrl = $"{origin}/nest/weather/v1?query=",
            UploadUrl = "",
            SoftwareUpdateUrl = "",
            ServerVersion = _apiSettings.ServerVersion,
            TierName = _apiSettings.TierName
        });
    }

    /// <summary>
    /// GET /nest/ping - Proxy to upstream ping endpoint
    /// </summary>
    [HttpGet("ping")]
    [RateLimit(MaxRequests = 120, WindowSeconds = 60, PolicyName = "nest-device")]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken)
    {
        var serial = ResolveDeviceSerial();

        if (CheckProxyAccess(serial) is { } accessDenied)
            return accessDenied;

        var body = await ReadRequestBodyAsync();
        _logger.LogInformation("[NestProxy] Ping from {Serial}", serial ?? "unknown");

        var proxyRequest = BuildProxyRequest("GET", "/nest/ping", serial, body);
        var response = await _proxyService.ProxyRequestAsync(proxyRequest, cancellationToken);

        return ProxyResponse(response);
    }

    /// <summary>
    /// GET /nest/passphrase - Proxy to upstream passphrase endpoint
    /// </summary>
    [HttpGet("passphrase")]
    [RateLimit(MaxRequests = 30, WindowSeconds = 60, PolicyName = "nest-device")]
    public async Task<IActionResult> GetPassphrase(CancellationToken cancellationToken)
    {
        var serial = ResolveDeviceSerial();

        if (CheckProxyAccess(serial) is { } accessDenied)
            return accessDenied;

        var body = await ReadRequestBodyAsync();
        _logger.LogInformation("[NestProxy] Passphrase request from {Serial}", serial ?? "unknown");

        var proxyRequest = BuildProxyRequest("GET", "/nest/passphrase", serial, body);
        var response = await _proxyService.ProxyRequestAsync(proxyRequest, cancellationToken);

        return ProxyResponse(response);
    }

    /// <summary>
    /// GET /nest/pro_info - Proxy to upstream pro_info endpoint
    /// </summary>
    [HttpGet("pro_info")]
    [HttpGet("pro-info")]
    [HttpGet("pro_info/{code}")]
    [HttpGet("pro-info/{code}")]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60, PolicyName = "nest-device")]
    public async Task<IActionResult> GetProInfo(
        [FromRoute] string? code,
        [FromQuery(Name = "code")] string? queryCode,
        [FromQuery(Name = "entry_code")] string? entryCode,
        CancellationToken cancellationToken)
    {
        var serial = ResolveDeviceSerial();

        if (CheckProxyAccess(serial) is { } accessDenied)
            return accessDenied;

        var resolvedCode = code ?? queryCode ?? entryCode;
        var path = string.IsNullOrWhiteSpace(resolvedCode) ? "/nest/pro_info" : $"/nest/pro_info/{resolvedCode}";

        var body = await ReadRequestBodyAsync();
        _logger.LogInformation("[NestProxy] ProInfo request from {Serial}, Code={Code}", serial ?? "unknown", resolvedCode ?? "none");

        var proxyRequest = BuildProxyRequest("GET", path, serial, body);
        var response = await _proxyService.ProxyRequestAsync(proxyRequest, cancellationToken);

        return ProxyResponse(response);
    }

    /// <summary>
    /// POST /nest/upload - Proxy to upstream upload endpoint
    /// </summary>
    [HttpPost("upload")]
    [RateLimit(MaxRequests = 30, WindowSeconds = 60, PolicyName = "nest-device")]
    public async Task<IActionResult> Upload(CancellationToken cancellationToken)
    {
        var serial = ResolveDeviceSerial();

        if (CheckProxyAccess(serial) is { } accessDenied)
            return accessDenied;

        var body = await ReadRequestBodyAsync();
        _logger.LogInformation("[NestProxy] Upload from {Serial}", serial ?? "unknown");

        var proxyRequest = BuildProxyRequest("POST", "/nest/upload", serial, body);
        var response = await _proxyService.ProxyRequestAsync(proxyRequest, cancellationToken);

        return ProxyResponse(response);
    }

    /// <summary>
    /// GET /nest/weather/v1 - Proxy to upstream weather endpoint
    /// </summary>
    [HttpGet("weather/v1")]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60, PolicyName = "nest-device")]
    public async Task<IActionResult> GetWeather([FromQuery] string? query, CancellationToken cancellationToken)
    {
        var serial = ResolveDeviceSerial();

        if (CheckProxyAccess(serial) is { } accessDenied)
            return accessDenied;

        if (string.IsNullOrEmpty(query))
        {
            if (Request.Query.TryGetValue("location", out var location) && !string.IsNullOrWhiteSpace(location))
            {
                query = location.ToString();
            }
            else if (Request.Query.TryGetValue("zip", out var zip) && !string.IsNullOrWhiteSpace(zip))
            {
                query = $"{zip},US";
            }
        }

        var body = await ReadRequestBodyAsync();
        _logger.LogInformation("[NestProxy] Weather request for {Query} from {Serial}", query ?? "none", serial ?? "unknown");

        var proxyRequest = BuildProxyRequest("GET", "/nest/weather/v1", serial, body);
        var response = await _proxyService.ProxyRequestAsync(proxyRequest, cancellationToken);

        return ProxyResponse(response);
    }

    /// <summary>
    /// GET /nest/transport/device/{serial} - Proxy to upstream transport device endpoint
    /// </summary>
    [HttpGet("transport/device/{serial}")]
    [HttpGet("transport/v7/device/{serial}")]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60, PolicyName = "nest-device")]
    public async Task<IActionResult> GetDeviceObjects(string serial, CancellationToken cancellationToken)
    {
        var authSerial = ResolveDeviceSerial();

        if (CheckProxyAccess(authSerial) is { } accessDenied)
            return accessDenied;

        var body = await ReadRequestBodyAsync();
        _logger.LogInformation("[NestProxy] Transport GET for device {Serial}", serial);

        var proxyRequest = BuildProxyRequest("GET", $"/nest/transport/device/{serial}", authSerial, body);
        var response = await _proxyService.ProxyRequestAsync(proxyRequest, cancellationToken);

        return ProxyResponse(response);
    }

    /// <summary>
    /// POST /nest/transport - Proxy to upstream transport subscribe endpoint with streaming
    /// </summary>
    [HttpPost("transport")]
    [HttpPost("transport/v7")]
    [HttpPost("transport/v7/subscribe")]
    [RateLimit(MaxRequests = 30, WindowSeconds = 60, PolicyName = "nest-device")]
    public async Task TransportSubscribe(CancellationToken cancellationToken)
    {
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        var serial = ResolveDeviceSerial();
        var settings = ProxySettings;

        if (!settings.Enabled)
        {
            Response.StatusCode = 503;
            Response.ContentType = "application/json";
            await Response.WriteAsync("{\"error\":\"Service temporarily unavailable\",\"code\":\"PROXY_DISABLED\"}", cancellationToken);
            return;
        }

        if (!settings.IsSerialAllowed(serial))
        {
            Response.StatusCode = 403;
            Response.ContentType = "application/json";
            await Response.WriteAsync("{\"error\":\"Device not authorized\",\"code\":\"SERIAL_NOT_ALLOWED\"}", cancellationToken);
            return;
        }

        var body = await ReadRequestBodyAsync();

        TransportSubscribeRequest? request = null;
        try
        {
            request = JsonSerializer.Deserialize<TransportSubscribeRequest>(body ?? "{}");
        }
        catch { }

        _logger.LogInformation("[NestProxy] Transport subscribe from {Serial}, chunked={Chunked}",
            serial ?? "unknown", request?.Chunked);

        var proxyRequest = BuildProxyRequest("POST", "/nest/transport", serial, body);

        if (request?.Chunked == true)
        {
            var streamingResponse = await _proxyService.InitiateStreamingRequestAsync(proxyRequest, cancellationToken);

            Response.StatusCode = streamingResponse.StatusCode;

            foreach (var header in streamingResponse.Headers)
            {
                if (IsHopByHopHeader(header.Key))
                    continue;
                Response.Headers[header.Key] = header.Value;
            }

            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";
            Response.Headers["X-Accel-Buffering"] = "no";

            await Response.StartAsync(cancellationToken);

            if (!streamingResponse.IsSuccess)
            {
                if (!string.IsNullOrEmpty(streamingResponse.Error))
                {
                    await Response.WriteAsync(streamingResponse.Error, cancellationToken);
                }
                return;
            }

            try
            {
                await foreach (var line in _proxyService.StreamResponseAsync(streamingResponse, cancellationToken))
                {
                    await Response.WriteAsync(line + "\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("[NestProxy] Subscription cancelled for {Serial}", serial);
            }
            finally
            {
                await Response.CompleteAsync();
            }
        }
        else
        {
            var response = await _proxyService.ProxyRequestAsync(proxyRequest, cancellationToken);

            Response.StatusCode = response.StatusCode;
            Response.ContentType = response.ContentType ?? "application/json";

            if (response.Headers is not null)
            {
                foreach (var header in response.Headers)
                {
                    if (IsHopByHopHeader(header.Key))
                        continue;
                    Response.Headers[header.Key] = header.Value;
                }
            }

            Response.Headers["X-Accel-Buffering"] = "no";

            if (!string.IsNullOrEmpty(response.Body))
            {
                await Response.StartAsync(cancellationToken);
                await Response.WriteAsync(response.Body, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
                await Response.CompleteAsync();
            }
        }
    }

    /// <summary>
    /// POST /nest/transport/put - Proxy to upstream transport put endpoint
    /// </summary>
    [HttpPost("transport/put")]
    [HttpPost("transport/v7/put")]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60, PolicyName = "nest-device")]
    public async Task<IActionResult> TransportPut(CancellationToken cancellationToken)
    {
        var serial = ResolveDeviceSerial();

        if (CheckProxyAccess(serial) is { } accessDenied)
            return accessDenied;

        var body = await ReadRequestBodyAsync();
        _logger.LogInformation("[NestProxy] Transport PUT from {Serial}", serial ?? "unknown");

        var proxyRequest = BuildProxyRequest("POST", "/nest/transport/put", serial, body);
        var response = await _proxyService.ProxyRequestAsync(proxyRequest, cancellationToken);

        return ProxyResponse(response);
    }

    #region Helpers

    private static bool IsHopByHopHeader(string key) =>
        key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("Connection", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("Host", StringComparison.OrdinalIgnoreCase);

    private NestProxyRequest BuildProxyRequest(string method, string path, string? serial, string? body = null)
    {
        var headers = new Dictionary<string, string>();

        foreach (var header in Request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            headers[header.Key] = header.Value.ToString();
        }

        var queryString = Request.QueryString.HasValue
            ? Request.QueryString.Value?.TrimStart('?')
            : null;

        return new NestProxyRequest
        {
            Method = method,
            Path = path,
            Headers = headers,
            Body = body,
            ContentType = Request.ContentType,
            DeviceSerial = serial,
            QueryString = queryString
        };
    }

    private async Task<string?> ReadRequestBodyAsync()
    {
        if (Request.ContentLength == 0)
            return null;

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Request.Body.Position = 0;
        return string.IsNullOrWhiteSpace(body) ? null : body;
    }

    private IActionResult ProxyResponse(NestProxyResponse response)
    {
        if (response.Headers is not null)
        {
            foreach (var header in response.Headers)
            {
                if (IsHopByHopHeader(header.Key))
                    continue;
                Response.Headers[header.Key] = header.Value;
            }
        }

        return new ContentResult
        {
            StatusCode = response.StatusCode,
            Content = response.Body,
            ContentType = response.ContentType ?? "application/json"
        };
    }

    private string? ResolveDeviceSerial()
    {
        if (Request.Headers.TryGetValue("X-NL-Device-Serial", out var serialHeader))
        {
            return serialHeader.FirstOrDefault();
        }

        if (Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var auth = authHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var encoded = auth["Basic ".Length..];
                    var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                    var parts = decoded.Split(':');
                    if (parts.Length >= 1 && !string.IsNullOrEmpty(parts[0]))
                    {
                        var username = parts[0];
                        if (username.StartsWith("nest.", StringComparison.OrdinalIgnoreCase))
                        {
                            username = username["nest.".Length..];
                        }
                        return username;
                    }
                }
                catch { }
            }
        }

        return null;
    }

    #endregion
}
