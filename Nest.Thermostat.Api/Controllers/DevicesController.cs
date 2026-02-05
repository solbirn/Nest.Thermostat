namespace Nest.Thermostat.Api.Controllers;

/// <summary>
/// Device management API for clients (authenticated)
/// </summary>
[ApiController]
[Route("devices")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(
        IDeviceRepository deviceRepository,
        ILogger<DevicesController> logger)
    {
        _deviceRepository = deviceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all registered devices
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDevices(CancellationToken cancellationToken)
    {
        var serials = await _deviceRepository.GetAllDeviceSerialsAsync(cancellationToken);
        
        var devices = new List<object>();
        foreach (var serial in serials)
        {
            var ownership = await _deviceRepository.GetDeviceOwnershipAsync(serial, cancellationToken);
            var deviceState = await _deviceRepository.GetDeviceStateAsync(serial, $"device.{serial}", cancellationToken);
            var sharedState = await _deviceRepository.GetDeviceStateAsync(serial, $"shared.{serial}", cancellationToken);

            devices.Add(new
            {
                serial,
                displayName = ownership?.DisplayName,
                location = ownership?.Location,
                claimedAt = ownership?.ClaimedAt,
                lastUpdated = deviceState?.UpdatedAt ?? sharedState?.UpdatedAt,
                hasDeviceState = deviceState is not null,
                hasSharedState = sharedState is not null
            });
        }

        return Ok(new { devices, count = devices.Count });
    }

    /// <summary>
    /// Get device details by serial
    /// </summary>
    [HttpGet("{serial}")]
    public async Task<IActionResult> GetDevice(string serial, CancellationToken cancellationToken)
    {
        var states = new Dictionary<string, object?>();
        
        await foreach (var state in _deviceRepository.GetAllDeviceStatesAsync(serial, cancellationToken))
        {
            states[state.ObjectKey] = new
            {
                objectKey = state.ObjectKey,
                objectRevision = state.ObjectRevision,
                objectTimestamp = state.ObjectTimestamp,
                value = state.GetValueAsJsonElement(),
                updatedAt = state.UpdatedAt
            };
        }

        if (states.Count == 0)
        {
            return NotFound(new { error = "Device not found", serial });
        }

        var ownership = await _deviceRepository.GetDeviceOwnershipAsync(serial, cancellationToken);

        return Ok(new
        {
            serial,
            ownership = ownership is null ? null : new
            {
                userId = ownership.UserId,
                userName = ownership.UserName,
                displayName = ownership.DisplayName,
                location = ownership.Location,
                claimedAt = ownership.ClaimedAt
            },
            states,
            objectCount = states.Count
        });
    }

    /// <summary>
    /// Get specific object for a device
    /// </summary>
    [HttpGet("{serial}/{objectType}")]
    public async Task<IActionResult> GetDeviceObject(string serial, string objectType, CancellationToken cancellationToken)
    {
        var objectKey = $"{objectType}.{serial}";
        var state = await _deviceRepository.GetDeviceStateAsync(serial, objectKey, cancellationToken);

        if (state is null)
        {
            return NotFound(new { error = "Object not found", serial, objectKey });
        }

        return Ok(new
        {
            objectKey = state.ObjectKey,
            objectRevision = state.ObjectRevision,
            objectTimestamp = state.ObjectTimestamp,
            value = state.GetValueAsJsonElement(),
            updatedAt = state.UpdatedAt
        });
    }

    /// <summary>
    /// Delete a device and all its data
    /// </summary>
    [HttpDelete("{serial}")]
    public async Task<IActionResult> DeleteDevice(string serial, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting device {Serial}", serial);
        
        var result = await _deviceRepository.DeleteDeviceAsync(serial, cancellationToken);
        
        if (!result)
        {
            return NotFound(new { error = "Device not found", serial });
        }

        return Ok(new { success = true, message = $"Device {serial} deleted" });
    }
}
