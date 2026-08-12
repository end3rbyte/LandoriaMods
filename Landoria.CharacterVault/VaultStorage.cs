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
        private const string CurrentFile = "current.fch";
        private const string BackupDirectory = "backups";
        private readonly string _root = Path.Combine(Paths.ConfigPath, "CharacterVault", "accounts");

        internal bool TryRead(string accountId, long characterId, out byte[] data, out long revision)
        {
            string directory = CharacterPath(accountId, characterId);
            string path = Path.Combine(directory, CurrentFile);
            data = File.Exists(path) && IsActive(directory) ? File.ReadAllBytes(path) : null;
            revision = data == null ? 0 : ReadRevision(directory);
            return data != null;
        }

        internal bool CanEnroll(string accountId, long characterId, bool allowMultiple)
        {
            string account = AccountPath(accountId);
            if (!Directory.Exists(account) || allowMultiple)
            {
                return true;
            }

            return IsActive(CharacterPath(accountId, characterId)) ||
                   !Directory.GetDirectories(account).Any(IsActive);
        }

        internal void Commit(string accountId, long characterId, string name, byte[] data,
            string hash, long revision)
        {
            string directory = CharacterPath(accountId, characterId);
            Directory.CreateDirectory(directory);
            string current = Path.Combine(directory, CurrentFile);
            string next = current + ".new";
            WriteDurably(next, data);
            if (File.Exists(current))
            {
                PreserveBackup(directory, current);
            }

            Replace(next, current);
            WriteMetadata(directory, name, hash, revision);
        }

        private static void PreserveBackup(string characterDirectory, string current)
        {
            string directory = Path.Combine(characterDirectory, BackupDirectory);
            Directory.CreateDirectory(directory);
            string timestamp = DateTime.UtcNow.ToString(
                "yyyyMMdd'T'HHmmssfffffff'Z'", CultureInfo.InvariantCulture);
            File.Copy(current, Path.Combine(directory, $"current-{timestamp}.fch"));
        }

        private string AccountPath(string accountId)
        {
            return Path.Combine(_root, Hash(Encoding.UTF8.GetBytes(accountId)));
        }

        private string CharacterPath(string accountId, long characterId)
        {
            return Path.Combine(AccountPath(accountId), characterId.ToString(CultureInfo.InvariantCulture));
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

        private static void WriteMetadata(string directory, string name, string hash, long revision)
        {
            string content = $"state=Active\nname={name}\nrevision={revision}\nhash={hash}\n";
            string path = Path.Combine(directory, "metadata.txt");
            string next = path + ".new";
            WriteDurably(next, Encoding.UTF8.GetBytes(content));
            Replace(next, path);
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
