using Nest.Thermostat.Core.Storage;

namespace Nest.Thermostat.Tests.Tests;

/// <summary>
/// Tests for the file-based document store
/// </summary>
[TestClass]
public class FileDocumentStoreTests
{
    private string _testDataPath = null!;
    private FileDocumentStore _store = null!;

    [TestInitialize]
    public void Setup()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"nest-thermostat-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDataPath);
        _store = new FileDocumentStore(new StorageSettings { BasePath = _testDataPath });
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, true);
        }
    }

    [TestMethod]
    public async Task UpsertAsync_CreatesDocument()
    {
        // Arrange
        var id = "test-doc-1";
        var doc = new TestDocument { Name = "Test", Value = 42 };

        // Act
        var result = await _store.UpsertAsync("test-container", id, doc);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
        result.Value.Should().Be(42);

        // Verify file exists
        var filePath = Path.Combine(_testDataPath, "test-container", $"{id}.json");
        File.Exists(filePath).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetAsync_ReturnsDocument()
    {
        // Arrange
        var id = "test-doc-2";
        var doc = new TestDocument { Name = "GetTest", Value = 100 };
        await _store.UpsertAsync("test-container", id, doc);

        // Act
        var result = await _store.GetAsync<TestDocument>("test-container", id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("GetTest");
        result.Value.Should().Be(100);
    }

    [TestMethod]
    public async Task GetAsync_NonExistent_ReturnsNull()
    {
        // Act
        var result = await _store.GetAsync<TestDocument>("test-container", "non-existent");

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesDocument()
    {
        // Arrange
        var id = "test-doc-3";
        var doc = new TestDocument { Name = "DeleteTest", Value = 1 };
        await _store.UpsertAsync("test-container", id, doc);

        // Act
        var deleted = await _store.DeleteAsync("test-container", id);

        // Assert
        deleted.Should().BeTrue();
        var result = await _store.GetAsync<TestDocument>("test-container", id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_NonExistent_ReturnsFalse()
    {
        // Act
        var deleted = await _store.DeleteAsync("test-container", "non-existent");

        // Assert
        deleted.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsAllDocuments()
    {
        // Arrange
        await _store.UpsertAsync("list-container", "doc-1", new TestDocument { Name = "A", Value = 1 });
        await _store.UpsertAsync("list-container", "doc-2", new TestDocument { Name = "B", Value = 2 });
        await _store.UpsertAsync("list-container", "doc-3", new TestDocument { Name = "C", Value = 3 });

        // Act
        var results = new List<TestDocument>();
        await foreach (var doc in _store.GetAllAsync<TestDocument>("list-container"))
        {
            results.Add(doc);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Select(d => d.Name).Should().Contain(new[] { "A", "B", "C" });
    }

    [TestMethod]
    public async Task QueryAsync_WithFilter_ReturnsMatchingDocuments()
    {
        // Arrange
        await _store.UpsertAsync("query-container", "doc-1", new TestDocument { Name = "Match", Value = 10 });
        await _store.UpsertAsync("query-container", "doc-2", new TestDocument { Name = "NoMatch", Value = 5 });
        await _store.UpsertAsync("query-container", "doc-3", new TestDocument { Name = "Match", Value = 20 });

        // Act
        var results = new List<TestDocument>();
        await foreach (var doc in _store.QueryAsync<TestDocument>("query-container", d => d.Name == "Match"))
        {
            results.Add(doc);
        }

        // Assert
        results.Should().HaveCount(2);
        results.All(d => d.Name == "Match").Should().BeTrue();
    }

    private record TestDocument
    {
        public string Name { get; init; } = "";
        public int Value { get; init; }
    }
}
