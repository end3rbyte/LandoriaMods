using Xunit;

namespace Landoria.CharacterVault;

public sealed class BackupRetentionEdgeTests
{
    private const string Profile = "Steam_1_Hero";

    // Verifies that the protected five most recent backups are never removed.
    [Fact]
    public void FiveOrFewerBackupsAreNeverDeleted()
    {
        WithDirectory(directory =>
        {
            WriteBackups(directory, Enumerable.Range(0, 5)
                .Select(index => new DateTime(2026, 8, 13, 15 - index, 0, 0, DateTimeKind.Utc)));

            Assert.Empty(BackupRetention.Apply(directory, Profile));
            Assert.Equal(5, Directory.GetFiles(directory).Length);
        });
    }

    // Verifies that retention caps a character's valid backup set at fifteen files.
    [Fact]
    public void RetentionNeverKeepsMoreThanFifteenBackups()
    {
        WithDirectory(directory =>
        {
            WriteBackups(directory, Enumerable.Range(0, 25)
                .Select(index => new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc)
                    .AddDays(-index)));

            IReadOnlyList<string> deleted = BackupRetention.Apply(directory, Profile);

            Assert.Equal(10, deleted.Count);
            Assert.Equal(15, Directory.GetFiles(directory).Length);
        });
    }

    // Verifies that another profile and malformed timestamps remain untouched.
    [Fact]
    public void UnrelatedAndMalformedFilesAreIgnored()
    {
        WithDirectory(directory =>
        {
            WriteBackups(directory, Enumerable.Range(0, 6)
                .Select(index => new DateTime(2026, 8, 13, 15 - index, 0, 0, DateTimeKind.Utc)));
            string otherProfile = Path.Combine(directory,
                "Steam_2_Other_20260813T1500000000000Z.fch");
            string malformed = Path.Combine(directory, Profile + "_not-a-date.fch");
            File.WriteAllText(otherProfile, "other");
            File.WriteAllText(malformed, "malformed");

            BackupRetention.Apply(directory, Profile);

            Assert.True(File.Exists(otherProfile));
            Assert.True(File.Exists(malformed));
        });
    }

    private static void WriteBackups(string directory, IEnumerable<DateTime> timestamps)
    {
        foreach (DateTime timestamp in timestamps)
        {
            string name = $"{Profile}_{timestamp:yyyyMMdd'T'HHmmssfffffff'Z'}.fch";
            File.WriteAllText(Path.Combine(directory, name), "backup");
        }
    }

    private static void WithDirectory(Action<string> test)
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            test(directory);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
