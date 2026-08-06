using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal sealed class SocialGroup
    {
        internal int Id;
        internal long Leader;
        internal readonly Dictionary<long, string> Members = new Dictionary<long, string>();
    }

    internal static class GroupState
    {
        internal const int MaximumSize = 5;
        internal static readonly Dictionary<int, SocialGroup> Groups =
            new Dictionary<int, SocialGroup>();
        internal static readonly Dictionary<long, int> PlayerGroups =
            new Dictionary<long, int>();
        internal static readonly Dictionary<long, long> PeerPlayers =
            new Dictionary<long, long>();
        internal static readonly Dictionary<long, long> Invitations =
            new Dictionary<long, long>();
        internal static readonly HashSet<long> LocalMembers = new HashSet<long>();

        internal static SocialGroup GetGroup(long playerId)
        {
            return PlayerGroups.TryGetValue(playerId, out int groupId) &&
                   Groups.TryGetValue(groupId, out SocialGroup group)
                ? group
                : null;
        }

        internal static int GetNextGroupId()
        {
            int highest = 0;
            foreach (int id in Groups.Keys)
            {
                highest = id > highest ? id : highest;
            }
            return highest + 1;
        }

        internal static void ClearServer()
        {
            Groups.Clear();
            PlayerGroups.Clear();
            PeerPlayers.Clear();
            Invitations.Clear();
        }

        internal static void ClearAll()
        {
            ClearServer();
            LocalMembers.Clear();
            GroupMapSharing.Clear();
        }
    }
}
