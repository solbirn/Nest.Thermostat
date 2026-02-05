using Nest.Thermostat.Core.Models;
using Nest.Thermostat.Core.Storage;

namespace Nest.Thermostat.Core.Repositories;

/// <summary>
/// Interface for device data repository operations.
/// </summary>
public interface IDeviceRepository
{
    Task<DeviceStateDocument?> GetDeviceStateAsync(string serial, string objectKey, CancellationToken ct = default);
    Task<DeviceStateDocument> UpsertDeviceStateAsync(DeviceStateDocument state, CancellationToken ct = default);
    IAsyncEnumerable<DeviceStateDocument> GetAllDeviceStatesAsync(string serial, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetAllDeviceSerialsAsync(CancellationToken ct = default);
    Task<DeviceOwnershipDocument?> GetDeviceOwnershipAsync(string serial, CancellationToken ct = default);
    Task<DeviceOwnershipDocument> UpsertDeviceOwnershipAsync(DeviceOwnershipDocument ownership, CancellationToken ct = default);
    Task<EntryKeyDocument?> GetEntryKeyAsync(string code, CancellationToken ct = default);
    Task<EntryKeyDocument> CreateEntryKeyAsync(EntryKeyDocument entryKey, CancellationToken ct = default);
    Task<bool> DeleteDeviceAsync(string serial, CancellationToken ct = default);
}

/// <summary>
/// File-based implementation of device repository.
/// </summary>
public class DeviceRepository : IDeviceRepository
{
    private readonly IDocumentStore _store;
    private const string DeviceStatesContainer = "device-states";
    private const string OwnershipContainer = "device-ownership";
    private const string EntryKeysContainer = "entry-keys";

    public DeviceRepository(IDocumentStore store)
    {
        _store = store;
    }

    public Task<DeviceStateDocument?> GetDeviceStateAsync(string serial, string objectKey, CancellationToken ct = default)
    {
        var id = $"{serial}_{objectKey.Replace(".", "_")}";
        return _store.GetAsync<DeviceStateDocument>(DeviceStatesContainer, id, ct);
    }

    public async Task<DeviceStateDocument> UpsertDeviceStateAsync(DeviceStateDocument state, CancellationToken ct = default)
    {
        var id = $"{state.Serial}_{state.ObjectKey.Replace(".", "_")}";
        state.Id = id;
        state.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return await _store.UpsertAsync(DeviceStatesContainer, id, state, ct);
    }

    public IAsyncEnumerable<DeviceStateDocument> GetAllDeviceStatesAsync(string serial, CancellationToken ct = default)
    {
        return _store.QueryAsync<DeviceStateDocument>(DeviceStatesContainer, s => s.Serial == serial, ct);
    }

    public async Task<IReadOnlyList<string>> GetAllDeviceSerialsAsync(CancellationToken ct = default)
    {
        var serials = new HashSet<string>();
        await foreach (var state in _store.GetAllAsync<DeviceStateDocument>(DeviceStatesContainer, ct))
        {
            serials.Add(state.Serial);
        }
        return serials.ToList();
    }

    public Task<DeviceOwnershipDocument?> GetDeviceOwnershipAsync(string serial, CancellationToken ct = default)
    {
        return _store.GetAsync<DeviceOwnershipDocument>(OwnershipContainer, serial, ct);
    }

    public async Task<DeviceOwnershipDocument> UpsertDeviceOwnershipAsync(DeviceOwnershipDocument ownership, CancellationToken ct = default)
    {
        ownership.Id = ownership.Serial;
        ownership.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return await _store.UpsertAsync(OwnershipContainer, ownership.Serial, ownership, ct);
    }

    public Task<EntryKeyDocument?> GetEntryKeyAsync(string code, CancellationToken ct = default)
    {
        return _store.GetAsync<EntryKeyDocument>(EntryKeysContainer, code, ct);
    }

    public async Task<EntryKeyDocument> CreateEntryKeyAsync(EntryKeyDocument entryKey, CancellationToken ct = default)
    {
        entryKey.Id = entryKey.Code;
        entryKey.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return await _store.UpsertAsync(EntryKeysContainer, entryKey.Code, entryKey, ct);
    }

    public async Task<bool> DeleteDeviceAsync(string serial, CancellationToken ct = default)
    {
        // Delete all device states for this serial
        var ids = new List<string>();
        await foreach (var id in _store.GetIdsAsync(DeviceStatesContainer, ct))
        {
            if (id.StartsWith($"{serial}_"))
                ids.Add(id);
        }

        foreach (var id in ids)
        {
            await _store.DeleteAsync(DeviceStatesContainer, id, ct);
        }

        // Delete ownership
        await _store.DeleteAsync(OwnershipContainer, serial, ct);

        return true;
    }
}
