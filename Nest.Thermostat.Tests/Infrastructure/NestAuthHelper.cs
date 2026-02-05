using System.Net.Http.Headers;
using System.Text;

namespace Nest.Thermostat.Tests.Infrastructure;

/// <summary>
/// Helper class for generating Nest API authentication headers
/// </summary>
public static class NestAuthHelper
{
    /// <summary>
    /// Creates a Basic Auth header with the nest.{SERIAL}:{password} format
    /// </summary>
    /// <param name="serial">Device serial number</param>
    /// <param name="password">Password (defaults to "password")</param>
    /// <param name="useNestPrefix">Whether to prefix serial with "nest." (default: true)</param>
    public static AuthenticationHeaderValue CreateBasicAuthHeader(string serial, string password = "password", bool useNestPrefix = true)
    {
        var username = useNestPrefix ? $"nest.{serial}" : serial;
        var credentials = $"{username}:{password}";
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        return new AuthenticationHeaderValue("Basic", base64Credentials);
    }

    /// <summary>
    /// Creates the raw Basic Auth header string for custom requests
    /// </summary>
    public static string CreateBasicAuthHeaderString(string serial, string password = "password", bool useNestPrefix = true)
    {
        var header = CreateBasicAuthHeader(serial, password, useNestPrefix);
        return $"{header.Scheme} {header.Parameter}";
    }

    /// <summary>
    /// Creates a malformed Basic Auth header for testing error cases
    /// </summary>
    public static AuthenticationHeaderValue CreateMalformedBasicAuthHeader()
    {
        // Invalid base64
        return new AuthenticationHeaderValue("Basic", "not-valid-base64!!!");
    }

    /// <summary>
    /// Creates a Basic Auth header with missing password
    /// </summary>
    public static AuthenticationHeaderValue CreateBasicAuthHeaderNoPassword(string serial, bool useNestPrefix = true)
    {
        var username = useNestPrefix ? $"nest.{serial}" : serial;
        var credentials = username; // No colon, no password
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        return new AuthenticationHeaderValue("Basic", base64Credentials);
    }

    /// <summary>
    /// Creates an empty Basic Auth header
    /// </summary>
    public static AuthenticationHeaderValue CreateEmptyBasicAuthHeader()
    {
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(":"));
        return new AuthenticationHeaderValue("Basic", base64Credentials);
    }
}
