namespace Nest.Thermostat.Tests.Infrastructure;

/// <summary>
/// Constants for the pre-seeded test device.
/// All tests can use this hardcoded device instead of generating random serials.
/// </summary>
public static class TestDeviceConstants
{
    /// <summary>
    /// Pre-seeded test device serial number
    /// </summary>
    public const string Serial = "09AA01ACTEST";

    /// <summary>
    /// Test user ID
    /// </summary>
    public const string UserId = "00000000-0000-0000-0000-000000000001";

    /// <summary>
    /// Test user display name
    /// </summary>
    public const string UserName = "Test User";

    /// <summary>
    /// Test user email
    /// </summary>
    public const string UserEmail = "testuser@nestthermostat.local";

    /// <summary>
    /// Entry key for claiming (if needed)
    /// </summary>
    public const string EntryKey = "123TEST1";

    /// <summary>
    /// Device display name
    /// </summary>
    public const string DisplayName = "Test Thermostat";

    /// <summary>
    /// Device location
    /// </summary>
    public const string Location = "Living Room";

    /// <summary>
    /// Default password for Basic Auth
    /// </summary>
    public const string Password = "password";
}
