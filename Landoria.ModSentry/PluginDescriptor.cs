namespace Landoria.ModSentry
{
    internal sealed class PluginDescriptor
    {
        internal PluginDescriptor(string guid, string name, string version, string hash,
            bool isBepInPlugin = true)
        {
            Guid = guid;
            Name = name;
            Version = version;
            Hash = hash;
            IsBepInPlugin = isBepInPlugin;
        }

        internal string Guid { get; }
        internal string Name { get; }
        internal string Version { get; }
        internal string Hash { get; }
        internal bool IsBepInPlugin { get; }
    }
}
