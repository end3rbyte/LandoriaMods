using System;

namespace Landoria.Socialize
{
    internal static class PieceAccessPolicy
    {
        internal static bool CanAccess(bool placedByPlayer, long playerId, long creator,
            bool hasMembershipState, Func<long, long, bool> areGroupMembers)
        {
            if (!placedByPlayer)
            {
                return true;
            }
            if (playerId == 0L)
            {
                return false;
            }
            if (creator == 0L || creator == playerId)
            {
                return true;
            }
            return hasMembershipState && areGroupMembers(playerId, creator);
        }
    }
}
