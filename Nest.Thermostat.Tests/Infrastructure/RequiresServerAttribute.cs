namespace Nest.Thermostat.Tests.Infrastructure;

/// <summary>
/// Attribute to mark tests that require a running server
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresServerAttribute : Attribute
{
    public string Url { get; }

    public RequiresServerAttribute(string url = NestApiClient.DefaultBaseUrl)
    {
        Url = url;
    }
}
