namespace Nest.Thermostat.Core.Storage;

/// <summary>
/// Configuration settings for file-based storage.
/// </summary>
public class StorageSettings
{
    /// <summary>
    /// Base path for all storage files.
    /// </summary>
    public string BasePath { get; set; } = "./data";

    /// <summary>
    /// Whether to format JSON with indentation for readability.
    /// </summary>
    public bool IndentJson { get; set; } = true;

    /// <summary>
    /// File extension for document files.
    /// </summary>
    public string FileExtension { get; set; } = ".json";
}
