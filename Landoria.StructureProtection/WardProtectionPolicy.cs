using System.Collections.Generic;

namespace Landoria.StructureProtection
{
    internal static class WardProtectionPolicy
    {
        internal static bool HasOnlineAuthorizedPlayer(
            long creator, IEnumerable<long> permittedPlayers, ISet<long> onlinePlayers)
        {
            if (onlinePlayers.Contains(creator))
            {
                return true;
            }
            foreach (long player in permittedPlayers)
            {
                if (onlinePlayers.Contains(player))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
