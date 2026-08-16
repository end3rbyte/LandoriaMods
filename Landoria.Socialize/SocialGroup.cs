using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal sealed class SocialGroup
    {
        internal const int MaximumSize = 5;
        internal int Id;
        internal long Leader;
        internal readonly Dictionary<long, string> Members =
            new Dictionary<long, string>();
        private readonly List<long> memberOrder = new List<long>();

        internal void AddMember(long playerId, string playerName)
        {
            if (!Members.ContainsKey(playerId))
            {
                memberOrder.Add(playerId);
            }
            Members[playerId] = playerName;
        }

        internal void RemoveMember(long playerId)
        {
            Members.Remove(playerId);
            memberOrder.Remove(playerId);
        }

        internal long GetOldestMember()
        {
            foreach (long playerId in memberOrder)
            {
                if (Members.ContainsKey(playerId))
                {
                    return playerId;
                }
            }
            return 0L;
        }
    }
}
