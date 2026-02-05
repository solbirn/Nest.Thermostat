namespace Nest.Thermostat.Core.Caching;

/// <summary>
/// Simple cache interface for in-memory caching.
/// </summary>
public interface ICache
{
    /// <summary>
    /// Get a cached value by key.
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// Try to get a cached value.
    /// </summary>
    bool TryGet<T>(string key, out T? value);

    /// <summary>
    /// Set a cached value with optional expiration.
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Get or create a cached value.
    /// </summary>
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Get or create a cached value asynchronously.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Remove a cached value.
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Clear all cached values.
    /// </summary>
    void Clear();
}
