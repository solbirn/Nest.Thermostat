using Microsoft.Extensions.Caching.Memory;

namespace Nest.Thermostat.Core.Caching;

/// <summary>
/// In-memory cache implementation using Microsoft.Extensions.Caching.Memory.
/// </summary>
public class InMemoryCache : ICache, IDisposable
{
    private readonly MemoryCache _cache;
    private readonly TimeSpan _defaultExpiration;

    public InMemoryCache(TimeSpan? defaultExpiration = null)
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(5);
    }

    public T? Get<T>(string key)
    {
        return _cache.TryGetValue(key, out T? value) ? value : default;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };
        _cache.Set(key, value, options);
    }

    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
    {
        return _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration;
            return factory();
        })!;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        return (await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration;
            return await factory();
        }))!;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public void Clear()
    {
        _cache.Compact(1.0);
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
