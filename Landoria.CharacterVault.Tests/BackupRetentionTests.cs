using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class BackupRetentionTests
{
    private const string Profile = "Steam_1_Hero";
    private static readonly string[] Backups =
    [
        "13/08/2026 15h", "13/08/2026 14h", "13/08/2026 13h", "13/08/2026 12h",
        "13/08/2026 11h", "13/08/2026 10h", "13/08/2026 09h", "13/08/2026 08h",
        "12/08/2026 15h", "12/08/2026 14h", "12/08/2026 13h", "12/08/2026 12h",
        "12/08/2026 11h", "11/08/2026 15h", "11/08/2026 14h", "11/08/2026 13h",
        "11/08/2026 12h", "11/08/2026 11h", "10/08/2026 15h", "10/08/2026 14h",
        "10/08/2026 13h", "10/08/2026 12h", "10/08/2026 11h", "09/08/2026 15h",
        "09/08/2026 14h", "09/08/2026 13h", "09/08/2026 12h", "09/08/2026 11h",
        "08/08/2026 15h", "08/08/2026 14h", "08/08/2026 13h", "08/08/2026 12h",
        "08/08/2026 11h", "07/08/2026 15h", "06/08/2026 14h", "05/08/2026 13h",
        "05/08/2026 12h", "04/08/2026 11h", "04/08/2026 10h", "03/08/2026 11h",
        "02/08/2026 11h", "01/08/2026 11h", "31/07/2026 11h", "30/07/2026 11h",
        "29/07/2026 11h"
    ];
    private static readonly string[] Retained =
    [
        "13/08/2026 15h", "13/08/2026 14h", "13/08/2026 13h", "13/08/2026 12h",
        "13/08/2026 11h", "12/08/2026 11h", "11/08/2026 11h", "10/08/2026 11h",
        "09/08/2026 11h", "08/08/2026 11h", "07/08/2026 15h", "06/08/2026 14h",
        "05/08/2026 12h", "04/08/2026 10h", "03/08/2026 11h"
    ];

    // Verifies the full retention example and confirms that reprocessing is idempotent.
    [Fact]
    public void ApplyExcludesTheFifthBackupsDayFromDailyRetention()
    {
        WithDirectory((directory, retention) =>
        {
            foreach (string timestamp in Backups)
            {
                File.WriteAllText(BackupPath(directory, timestamp), "backup");
            }

            IReadOnlyList<string> deleted = retention.Apply(directory, Profile);
            string[] retainedNames = Retained.Select(value =>
                Path.GetFileName(BackupPath(directory, value))).ToArray();
            string[] deletedNames = Backups.Except(Retained).Select(value =>
                Path.GetFileName(BackupPath(directory, value))).ToArray();
            Assert.Equal(30, deleted.Count);
            Assert.Equal(deletedNames.OrderBy(name => name), deleted.OrderBy(name => name));
            Assert.Equal(retainedNames.OrderBy(name => name), Directory.GetFiles(directory)
                .Select(Path.GetFileName).OrderBy(name => name));

            Assert.Empty(retention.Apply(directory, Profile));
        });
    }

    private static string BackupPath(string directory, string value)
    {
        DateTime timestamp = DateTime.ParseExact(value, "dd/MM/yyyy HH'h'",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        return Path.Combine(directory, $"{Profile}_{timestamp:yyyyMMdd'T'HHmmssfffffff'Z'}.fch");
    }

    internal static void WithDirectory(Action<string, BackupRetention> test)
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        using ServiceProvider services = new ServiceCollection()
            .AddSingleton<IBackupFileSystem, SystemBackupFileSystem>()
            .AddSingleton(provider => new BackupRetention(
                provider.GetRequiredService<IBackupFileSystem>()))
            .BuildServiceProvider();
        try
        {
            test(directory, services.GetRequiredService<BackupRetention>());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
