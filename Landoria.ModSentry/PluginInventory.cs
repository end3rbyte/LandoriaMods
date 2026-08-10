using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BepInEx.Bootstrap;

namespace Landoria.ModSentry
{
    internal static class PluginInventory
    {
        internal static IReadOnlyList<PluginDescriptor> Capture()
        {
            return Chainloader.PluginInfos.Values
                .Select(info => Create(info.Metadata.GUID, info.Metadata.Name,
                    info.Metadata.Version.ToString(), info.Location))
                .OrderBy(plugin => plugin.Guid, StringComparer.Ordinal)
                .ToList();
        }

        internal static ZPackage Serialize()
        {
            IReadOnlyList<PluginDescriptor> plugins = Capture();
            ZPackage package = new ZPackage();
            package.Write(ModSentryPlugin.ProtocolVersion);
            package.Write(plugins.Count);
            foreach (PluginDescriptor plugin in plugins)
            {
                Write(package, plugin);
            }

            return package;
        }

        internal static List<PluginDescriptor> Deserialize(ZPackage package)
        {
            int protocol = package.ReadInt();
            if (protocol != ModSentryPlugin.ProtocolVersion)
            {
                throw new InvalidDataException($"Unsupported ModSentry protocol {protocol}.");
            }

            int count = package.ReadInt();
            if (count < 0 || count > 1024)
            {
                throw new InvalidDataException("The client plugin count is invalid.");
            }

            List<PluginDescriptor> plugins = new List<PluginDescriptor>(count);
            for (int index = 0; index < count; index++)
            {
                plugins.Add(Read(package));
            }

            return plugins;
        }

        internal static string Sha256(string path)
        {
            using (SHA256 algorithm = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "");
            }
        }

        private static PluginDescriptor Create(string guid, string name, string version, string path)
        {
            return new PluginDescriptor(guid, name, version, Sha256(path));
        }

        private static void Write(ZPackage package, PluginDescriptor plugin)
        {
            package.Write(plugin.Guid);
            package.Write(plugin.Name);
            package.Write(plugin.Version);
            package.Write(plugin.Hash);
        }

        private static PluginDescriptor Read(ZPackage package)
        {
            return new PluginDescriptor(package.ReadString(), package.ReadString(),
                package.ReadString(), package.ReadString());
        }
    }
}
