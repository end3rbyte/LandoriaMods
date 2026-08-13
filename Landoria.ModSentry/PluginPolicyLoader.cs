using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Mono.Cecil;

namespace Landoria.ModSentry
{
    internal static class PluginPolicyLoader
    {
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
                throw new DirectoryNotFoundException(
                    $"ModSentry policy directory is missing: {directory}");
            }

            return Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly)
                .Select(ReadDescriptor)
                .OrderBy(plugin => plugin.Guid, StringComparer.Ordinal)
                .ToList();
        }

        private static PluginDescriptor ReadDescriptor(string path)
        {
            using (AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path))
            {
                CustomAttribute attribute = assembly.MainModule.Types
                    .SelectMany(type => type.CustomAttributes)
                    .SingleOrDefault(item =>
                        item.AttributeType.FullName == typeof(BepInPlugin).FullName);
                return CreateDescriptor(path, attribute);
            }
        }

        private static PluginDescriptor CreateDescriptor(string path, CustomAttribute attribute)
        {
            if (attribute == null || attribute.ConstructorArguments.Count < 3)
            {
                throw new InvalidDataException(
                    $"No BepInPlugin metadata was found in {Path.GetFileName(path)}.");
            }

            string guid = (string)attribute.ConstructorArguments[0].Value;
            string name = (string)attribute.ConstructorArguments[1].Value;
            string version = (string)attribute.ConstructorArguments[2].Value;
            return new PluginDescriptor(guid, name, version, PluginInventory.Sha256(path));
        }
    }
}
