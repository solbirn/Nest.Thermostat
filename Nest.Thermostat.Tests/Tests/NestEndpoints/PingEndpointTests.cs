namespace Nest.Thermostat.Tests.Tests.NestEndpoints;

/// <summary>
/// Tests for GET /nest/ping - Health check endpoint
/// </summary>
[TestClass]
[RequiresServer]
public class PingEndpointTests : IDisposable
{
    private readonly NestApiClient _client;

    public PingEndpointTests()
    {
        _client = new NestApiClient();
    }

    #region Basic Response Tests

    [TestMethod]
    [Ignore("Requires running server")]
    public async Task GetPing_WithoutAuth_ReturnsOk()
    {
        // Arrange
        _client.ClearAuth();

        // Act
        var response = await _client.GetPingAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    [Ignore("Requires running server")]
    public async Task GetPing_WithAuth_ReturnsOk()
    {
        // Arrange
        var serial = TestSerialGenerator.Generate();
        _client.SetBasicAuth(serial);

        // Act
        var response = await _client.GetPingAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    [Ignore("Requires running server")]
    public async Task GetPing_ReturnsJsonContentType()
    {
        // Arrange
        _client.ClearAuth();

        // Act
        var response = await _client.GetPingAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    #endregion

    #region Response Structure Tests

    [TestMethod]
    [Ignore("Requires running server")]
    public async Task GetPing_ReturnsValidJson()
    {
        // Arrange
        _client.ClearAuth();

        // Act
        var response = await _client.GetPingAsync();
        var content = await NestApiClient.GetResponseContentAsync(response);

        // Assert
        var parseAction = () => JsonDocument.Parse(content);
        parseAction.Should().NotThrow("ping response should be valid JSON");
    }

    [TestMethod]
    [Ignore("Requires running server")]
    public async Task GetPing_ReturnsStatusField()
    {
        // Arrange
        _client.ClearAuth();

        // Act
        var response = await _client.GetPingAsync();
        var content = await NestApiClient.GetResponseContentAsync(response);
        var json = JsonDocument.Parse(content);

        // Assert
        json.RootElement.TryGetProperty("status", out var status).Should().BeTrue("should have status field");
        status.GetString().Should().Be("ok", "status should be 'ok'");
    }

    #endregion

    #region Performance Tests

    [TestMethod]
    [Ignore("Requires running server")]
    public async Task GetPing_ReturnsQuickResponse()
    {
        // Arrange
        _client.ClearAuth();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetPingAsync();
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "ping should respond quickly");
    }

    #endregion

    public void Dispose()
    {
        _client.Dispose();
    }
}
