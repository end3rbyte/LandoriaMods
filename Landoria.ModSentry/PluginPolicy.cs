using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;

namespace Landoria.ModSentry
{
    internal sealed class PluginPolicy
    {
        internal PluginPolicy(IReadOnlyList<PluginDescriptor> required,
            IReadOnlyList<PluginDescriptor> optional)
        {
            Required = required;
            Optional = optional;
        }

        internal IReadOnlyList<PluginDescriptor> Required { get; }
        internal IReadOnlyList<PluginDescriptor> Optional { get; }

        internal static PluginPolicy Load()
        {
            return new PluginPolicy(
                LoadDirectory(Path.Combine(Paths.ConfigPath, "ModSentry_Required")),
                LoadOptionalPolicy(Path.Combine(Paths.ConfigPath, "ModSentry_Optional.policy")));
        }

        private static IReadOnlyList<PluginDescriptor> LoadOptionalPolicy(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("ModSentry optional policy is missing.", path);
            }

            return File.ReadAllLines(path)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(ParseDescriptor)
                .OrderBy(plugin => plugin.Guid, StringComparer.Ordinal)
                .ToList();
        }

        private static PluginDescriptor ParseDescriptor(string line)
        {
            string[] fields = line.Split('|');
            if (fields.Length != 4 || fields.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidDataException("A ModSentry optional policy entry is invalid.");
            }

            return new PluginDescriptor(fields[0], fields[1], fields[2], fields[3]);
        }

        private static IReadOnlyList<PluginDescriptor> LoadDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"ModSentry policy directory is missing: {directory}");
            }

            return Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly)
                .Select(ReadDescriptor)
                .OrderBy(plugin => plugin.Guid, StringComparer.Ordinal)
                .ToList();
        }

        private static PluginDescriptor ReadDescriptor(string path)
        {
            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(path);
            CustomAttributeData attribute = assembly.GetCustomAttributesData()
                .SingleOrDefault(item => item.AttributeType.FullName == typeof(BepInPlugin).FullName);
            if (attribute == null || attribute.ConstructorArguments.Count < 3)
            {
                throw new InvalidDataException($"No BepInPlugin metadata was found in {Path.GetFileName(path)}.");
            }

            string guid = (string)attribute.ConstructorArguments[0].Value;
            string name = (string)attribute.ConstructorArguments[1].Value;
            string version = (string)attribute.ConstructorArguments[2].Value;
            return new PluginDescriptor(guid, name, version, PluginInventory.Sha256(path));
        }
    }
}
