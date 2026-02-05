namespace Nest.Thermostat.Core.Storage;

/// <summary>
/// Interface for document storage operations.
/// Provides CRUD operations for JSON documents organized in containers.
/// </summary>
public interface IDocumentStore
{
    /// <summary>
    /// Get a document by container and ID.
    /// </summary>
    Task<T?> GetAsync<T>(string container, string id, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Get all documents in a container.
    /// </summary>
    IAsyncEnumerable<T> GetAllAsync<T>(string container, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Query documents in a container with an optional filter.
    /// </summary>
    IAsyncEnumerable<T> QueryAsync<T>(string container, Func<T, bool>? filter = null, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Upsert a document (insert or update).
    /// </summary>
    Task<T> UpsertAsync<T>(string container, string id, T document, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Delete a document.
    /// </summary>
    Task<bool> DeleteAsync(string container, string id, CancellationToken ct = default);

    /// <summary>
    /// Check if a document exists.
    /// </summary>
    Task<bool> ExistsAsync(string container, string id, CancellationToken ct = default);

    /// <summary>
    /// Get all document IDs in a container.
    /// </summary>
    IAsyncEnumerable<string> GetIdsAsync(string container, CancellationToken ct = default);
}
