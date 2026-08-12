using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BepInEx;

namespace Landoria.CharacterVault
{
    internal sealed class VaultStorage
    {
        private const string LegacyCurrentFile = "current.fch";
        private const string BackupDirectory = "backups";
        private readonly string _root = Path.Combine(
            Utils.GetSaveDataPath(FileHelpers.FileSource.Local), "characters_local");
        private readonly string _legacyRoot = Path.Combine(
            Paths.ConfigPath, "CharacterVault", "accounts");

        internal bool TryRead(
            string accountId, long characterId, string name, out byte[] data, out long revision)
        {
            string path = ProfilePath(accountId, name);
            if (File.Exists(path))
            {
                data = File.ReadAllBytes(path);
                revision = 0;
                return true;
            }

            return TryReadLegacy(accountId, characterId, out data, out revision);
        }

        internal bool CanEnroll(string accountId, string name, bool allowMultiple)
        {
            if (allowMultiple || File.Exists(ProfilePath(accountId, name)))
            {
                return true;
            }

            string prefix = SafeSegment(accountId) + "_";
            bool currentExists = Directory.Exists(_root) && Directory
                .GetFiles(_root, prefix + "*.fch", SearchOption.TopDirectoryOnly).Any();
            string legacyAccount = LegacyAccountPath(accountId);
            bool legacyExists = Directory.Exists(legacyAccount) &&
                Directory.GetDirectories(legacyAccount).Any(IsActive);
            return !currentExists && !legacyExists;
        }

        internal void Commit(string accountId, string name, byte[] data)
        {
            Directory.CreateDirectory(_root);
            string current = ProfilePath(accountId, name);
            string next = current + ".new";
            WriteDurably(next, data);
            PreserveBackup(data, Path.GetFileNameWithoutExtension(current));
            Replace(next, current);
        }

        private void PreserveBackup(byte[] data, string profileName)
        {
            string directory = Path.Combine(_root, BackupDirectory);
            Directory.CreateDirectory(directory);
            string timestamp = DateTime.UtcNow.ToString(
                "yyyyMMdd'T'HHmmssfffffff'Z'", CultureInfo.InvariantCulture);
            WriteDurably(Path.Combine(directory, $"{profileName}_{timestamp}.fch"), data);
        }

        private bool TryReadLegacy(
            string accountId, long characterId, out byte[] data, out long revision)
        {
            string directory = LegacyCharacterPath(accountId, characterId);
            string path = FindLegacyProfile(accountId, characterId);
            data = File.Exists(path) && IsActive(directory) ? File.ReadAllBytes(path) : null;
            revision = data == null ? 0 : ReadRevision(directory);
            return data != null;
        }

        private static string ProfileFileName(string accountId, string name)
        {
            return $"{SafeSegment(accountId)}_{SafeSegment(name)}.fch";
        }

        private static string SafeSegment(string value)
        {
            const string invalid = "<>:\"/\\|?*";
            return new string(value
                .Select(character => char.IsControl(character) || invalid.Contains(character)
                    ? '_' : character)
                .ToArray());
        }

        private string ProfilePath(string accountId, string name)
        {
            return Path.Combine(_root, ProfileFileName(accountId, name));
        }

        private string LegacyAccountPath(string accountId)
        {
            return Path.Combine(_legacyRoot, Hash(Encoding.UTF8.GetBytes(accountId)));
        }

        private string LegacyCharacterPath(string accountId, long characterId) => Path.Combine(
            LegacyAccountPath(accountId), characterId.ToString(CultureInfo.InvariantCulture));

        private string FindLegacyProfile(string accountId, long characterId)
        {
            string directory = LegacyCharacterPath(accountId, characterId);
            if (!Directory.Exists(directory)) return Path.Combine(directory, LegacyCurrentFile);
            return Directory.GetFiles(directory, "*.fch", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path => Path.GetFileName(path) != LegacyCurrentFile) ??
                Path.Combine(directory, LegacyCurrentFile);
        }

        private static void WriteDurably(string path, byte[] data)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write,
                FileShare.None, 65536, FileOptions.WriteThrough))
            {
                stream.Write(data, 0, data.Length);
                stream.Flush(true);
            }
        }

        private static long ReadRevision(string directory)
        {
            string path = Path.Combine(directory, "metadata.txt");
            if (!File.Exists(path))
            {
                return 0;
            }

            foreach (string line in File.ReadAllLines(path))
            {
                if (line.StartsWith("revision=", StringComparison.Ordinal) &&
                    long.TryParse(line.Substring(9), NumberStyles.None,
                        CultureInfo.InvariantCulture, out long revision))
                {
                    return revision;
                }
            }

            return 0;
        }

        private static bool IsActive(string directory)
        {
            string path = Path.Combine(directory, "metadata.txt");
            return File.Exists(path) && File.ReadLines(path).Any(line =>
                string.Equals(line, "state=Active", StringComparison.Ordinal));
        }

        private static void Replace(string source, string destination)
        {
            if (File.Exists(destination))
            {
                File.Replace(source, destination, null);
            }
            else
            {
                File.Move(source, destination);
            }
        }

        internal static string Hash(byte[] data)
        {
            using (SHA256 algorithm = SHA256.Create())
            {
                return BitConverter.ToString(algorithm.ComputeHash(data)).Replace("-", string.Empty);
            }
        }
    }
}
