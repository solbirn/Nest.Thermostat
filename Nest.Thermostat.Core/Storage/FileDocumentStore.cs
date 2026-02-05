using Microsoft.Extensions.Options;

namespace Nest.Thermostat.Core.Storage;

/// <summary>
/// File-based implementation of IDocumentStore.
/// Stores JSON documents as files in a directory hierarchy.
/// Container = folder, Document ID = filename (without extension).
/// </summary>
public class FileDocumentStore : IDocumentStore
{
    private readonly StorageSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileDocumentStore(IOptions<StorageSettings> settings)
    {
        _settings = settings.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = _settings.IndentJson
        };
    }

    public FileDocumentStore(StorageSettings settings)
    {
        _settings = settings;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = _settings.IndentJson
        };
    }

    private string GetContainerPath(string container) =>
        Path.Combine(_settings.BasePath, SanitizePath(container));

    private string GetDocumentPath(string container, string id) =>
        Path.Combine(GetContainerPath(container), $"{SanitizePath(id)}{_settings.FileExtension}");

    private static string SanitizePath(string input) =>
        string.Join("_", input.Split(Path.GetInvalidFileNameChars()));

    public async Task<T?> GetAsync<T>(string container, string id, CancellationToken ct = default) where T : class
    {
        var path = GetDocumentPath(container, id);
        if (!File.Exists(path))
            return null;

        await _lock.WaitAsync(ct);
        try
        {
            var json = await File.ReadAllTextAsync(path, ct);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async IAsyncEnumerable<T> GetAllAsync<T>(string container, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default) where T : class
    {
        var containerPath = GetContainerPath(container);
        if (!Directory.Exists(containerPath))
            yield break;

        var files = Directory.GetFiles(containerPath, $"*{_settings.FileExtension}");
        foreach (var file in files)
        {
            if (ct.IsCancellationRequested)
                yield break;

            await _lock.WaitAsync(ct);
            try
            {
                var json = await File.ReadAllTextAsync(file, ct);
                var doc = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                if (doc is not null)
                    yield return doc;
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    public async IAsyncEnumerable<T> QueryAsync<T>(string container, Func<T, bool>? filter = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default) where T : class
    {
        await foreach (var doc in GetAllAsync<T>(container, ct))
        {
            if (filter is null || filter(doc))
                yield return doc;
        }
    }

    public async Task<T> UpsertAsync<T>(string container, string id, T document, CancellationToken ct = default) where T : class
    {
        var containerPath = GetContainerPath(container);
        Directory.CreateDirectory(containerPath);

        var path = GetDocumentPath(container, id);
        var json = JsonSerializer.Serialize(document, _jsonOptions);

        await _lock.WaitAsync(ct);
        try
        {
            await File.WriteAllTextAsync(path, json, ct);
            return document;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(string container, string id, CancellationToken ct = default)
    {
        var path = GetDocumentPath(container, id);
        if (!File.Exists(path))
            return false;

        await _lock.WaitAsync(ct);
        try
        {
            File.Delete(path);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<bool> ExistsAsync(string container, string id, CancellationToken ct = default)
    {
        var path = GetDocumentPath(container, id);
        return Task.FromResult(File.Exists(path));
    }

    public async IAsyncEnumerable<string> GetIdsAsync(string container, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var containerPath = GetContainerPath(container);
        if (!Directory.Exists(containerPath))
            yield break;

        await Task.CompletedTask; // Make async

        var files = Directory.GetFiles(containerPath, $"*{_settings.FileExtension}");
        foreach (var file in files)
        {
            if (ct.IsCancellationRequested)
                yield break;

            yield return Path.GetFileNameWithoutExtension(file);
        }
    }
}
