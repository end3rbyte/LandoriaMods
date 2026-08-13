using System.Collections.Generic;

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
    }
}
