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
                LoadDirectory(Path.Combine(Paths.ConfigPath, "ModSentry_Optional")));
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
