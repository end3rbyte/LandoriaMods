using System.Collections.Generic;

namespace Landoria.DecayControl
{
    internal static class CreatorActivityPolicy
    {
        internal static HashSet<long> GetActiveCreators(IEnumerable<long> onlinePlayers)
        {
            return new HashSet<long>(onlinePlayers);
        }

        internal static bool IsCreatorActive(long creator, ISet<long> onlinePlayers)
        {
            return onlinePlayers.Contains(creator);
        }
    }
}
