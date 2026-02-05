using System.Net.Http.Headers;
using System.Text;

namespace Nest.Thermostat.Tests.Infrastructure;

/// <summary>
/// HTTP client wrapper configured to target the Nest API
/// </summary>
public class NestApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly TimeSpan[] TooManyRequestsRetryDelays =
    [
        TimeSpan.FromMilliseconds(100),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2)
    ];

    public const string DefaultBaseUrl = "https://localhost:5001";
    public const string NestBasePath = "/nest";

    public string BaseUrl { get; }

    public NestApiClient(string? baseUrl = null)
    {
        BaseUrl = baseUrl ?? DefaultBaseUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Sets Basic Auth header for device authentication
    /// </summary>
    public void SetBasicAuth(string serial, string password = "password", bool useNestPrefix = true)
    {
        _httpClient.DefaultRequestHeaders.Authorization = NestAuthHelper.CreateBasicAuthHeader(serial, password, useNestPrefix);
    }

    /// <summary>
    /// Sets X-NL-Device-Serial header for device authentication
    /// </summary>
    public void SetDeviceSerialHeader(string serial)
    {
        _httpClient.DefaultRequestHeaders.Remove("X-NL-Device-Serial");
        _httpClient.DefaultRequestHeaders.Add("X-NL-Device-Serial", serial);
    }

    /// <summary>
    /// Clears all authentication headers
    /// </summary>
    public void ClearAuth()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _httpClient.DefaultRequestHeaders.Remove("X-NL-Device-Serial");
    }

    #region Nest Endpoints

    /// <summary>
    /// GET /nest/entry - Service discovery endpoint
    /// </summary>
    public async Task<HttpResponseMessage> GetEntryAsync()
    {
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.GetAsync($"{NestBasePath}/entry"));
    }

    /// <summary>
    /// GET /nest/ping - Health check endpoint
    /// </summary>
    public async Task<HttpResponseMessage> GetPingAsync()
    {
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.GetAsync($"{NestBasePath}/ping"));
    }

    /// <summary>
    /// GET /nest/passphrase - Get/generate entry key
    /// </summary>
    public async Task<HttpResponseMessage> GetPassphraseAsync()
    {
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.GetAsync($"{NestBasePath}/passphrase"));
    }

    /// <summary>
    /// GET /nest/pro_info - Get installer information
    /// </summary>
    public async Task<HttpResponseMessage> GetProInfoAsync()
    {
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.GetAsync($"{NestBasePath}/pro_info"));
    }

    /// <summary>
    /// GET /nest/pro_info/{code} - Get installer information by code
    /// </summary>
    public async Task<HttpResponseMessage> GetProInfoWithCodeAsync(string code)
    {
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.GetAsync($"{NestBasePath}/pro_info/{Uri.EscapeDataString(code)}"));
    }

    /// <summary>
    /// GET /nest/weather/v1 - Weather API proxy
    /// </summary>
    public async Task<HttpResponseMessage> GetWeatherAsync(string? query = null)
    {
        var path = $"{NestBasePath}/weather/v1";
        if (!string.IsNullOrEmpty(query))
        {
            path += $"?{query}";
        }
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.GetAsync(path));
    }

    /// <summary>
    /// POST /nest/upload - Upload endpoint
    /// </summary>
    public async Task<HttpResponseMessage> PostUploadAsync(HttpContent? content = null)
    {
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.PostAsync($"{NestBasePath}/upload", content ?? new StringContent("")));
    }

    /// <summary>
    /// GET /nest/transport/device/{serial} - Get device objects
    /// </summary>
    public async Task<HttpResponseMessage> GetTransportAsync(string serial)
    {
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.GetAsync($"{NestBasePath}/transport/device/{serial}"));
    }

    /// <summary>
    /// POST /nest/transport - Subscribe to state updates
    /// </summary>
    public async Task<HttpResponseMessage> PostTransportSubscribeAsync(object request)
    {
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.PostAsJsonAsync($"{NestBasePath}/transport", request, _jsonOptions));
    }

    /// <summary>
    /// POST /nest/transport/put - Submit updated object values
    /// </summary>
    public async Task<HttpResponseMessage> PostTransportPutAsync(object request)
    {
        return await ExecuteWithTooManyRequestsRetryAsync(() => _httpClient.PostAsJsonAsync($"{NestBasePath}/transport/put", request, _jsonOptions));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Deserialize response content as JSON
    /// </summary>
    public async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    /// <summary>
    /// Get raw response content as string
    /// </summary>
    public static async Task<string> GetResponseContentAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    private static TimeSpan? TryGetRetryAfterDelay(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter is null)
        {
            return null;
        }

        if (retryAfter.Delta is not null)
        {
            return retryAfter.Delta.Value;
        }

        if (retryAfter.Date is not null)
        {
            var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
        }

        return null;
    }

    private static async Task<HttpResponseMessage> ExecuteWithTooManyRequestsRetryAsync(Func<Task<HttpResponseMessage>> sendAsync)
    {
        for (var attempt = 0; attempt < TooManyRequestsRetryDelays.Length; attempt++)
        {
            var response = await sendAsync();
            if (response.StatusCode != HttpStatusCode.TooManyRequests)
            {
                return response;
            }

            var serverDelay = TryGetRetryAfterDelay(response);
            response.Dispose();

            var delay = serverDelay ?? TooManyRequestsRetryDelays[attempt];
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }
        }

        return await sendAsync();
    }

    #endregion

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
