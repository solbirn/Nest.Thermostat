using Bogus;

namespace Nest.Thermostat.Tests.Infrastructure;

/// <summary>
/// Generates unique device serial numbers for test isolation
/// </summary>
public static class TestSerialGenerator
{
    private static readonly Faker Faker = new();
    private static int _counter;

    /// <summary>
    /// Generates a unique 12-character serial number
    /// Format: TEST + 8 alphanumeric chars based on timestamp + counter
    /// </summary>
    public static string Generate()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var counter = Interlocked.Increment(ref _counter);
        var suffix = $"{timestamp:X8}{counter:X4}";
        return $"TEST{suffix[^8..].ToUpperInvariant()}";
    }

    /// <summary>
    /// Generates a unique serial with a custom prefix (max 4 chars)
    /// </summary>
    public static string GenerateWithPrefix(string prefix)
    {
        var safePrefix = prefix.Length > 4 ? prefix[..4] : prefix;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var counter = Interlocked.Increment(ref _counter);
        var remaining = 12 - safePrefix.Length;
        var suffix = $"{timestamp:X8}{counter:X4}";
        return $"{safePrefix.ToUpperInvariant()}{suffix[^remaining..].ToUpperInvariant()}";
    }

    /// <summary>
    /// Generates a random serial using Bogus (for variety in tests)
    /// </summary>
    public static string GenerateRandom()
    {
        return Faker.Random.AlphaNumeric(12).ToUpperInvariant();
    }

    /// <summary>
    /// Generates a batch of unique serials
    /// </summary>
    public static string[] GenerateBatch(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => Generate())
            .ToArray();
    }

    /// <summary>
    /// Generates a serial that looks like a real Nest device serial
    /// Format: 09AA01AC + 4 random hex digits (Nest-like format)
    /// </summary>
    public static string GenerateNestLike()
    {
        var suffix = Faker.Random.Hexadecimal(4, "").ToUpperInvariant();
        return $"09AA01AC{suffix}";
    }
}
