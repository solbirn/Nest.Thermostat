using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace Nest.Thermostat.Api.Infrastructure;

/// <summary>
/// Rate limiting attribute for protecting endpoints from abuse.
/// Uses IP-based rate limiting with a sliding window.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Maximum number of requests allowed within the time window
    /// </summary>
    public int MaxRequests { get; set; } = 60;

    /// <summary>
    /// Time window in seconds
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Optional policy name for categorization
    /// </summary>
    public string? PolicyName { get; set; }

    private static readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = context.ActionDescriptor.DisplayName ?? "unknown";
        var key = $"ratelimit:{PolicyName ?? "default"}:{ipAddress}:{endpoint}";

        var windowStart = DateTimeOffset.UtcNow.AddSeconds(-WindowSeconds);
        var cacheKey = $"{key}:{windowStart.ToUnixTimeSeconds() / WindowSeconds}";

        var requestCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(WindowSeconds);
            return 0;
        });

        if (requestCount >= MaxRequests)
        {
            context.Result = new ContentResult
            {
                StatusCode = (int)HttpStatusCode.TooManyRequests,
                Content = JsonSerializer.Serialize(new { error = "Too many requests", retryAfter = WindowSeconds }),
                ContentType = "application/json"
            };
            return;
        }

        _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromSeconds(WindowSeconds));
        await next();
    }
}
