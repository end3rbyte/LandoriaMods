using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal sealed class GroupRemovalResult
    {
        internal bool Disbanded;
        internal long NewLeader;
        internal readonly List<long> RemainingMembers = new List<long>();
    }

    internal static class GroupLifecyclePolicy
    {
        internal static GroupRemovalResult Remove(SocialGroup group, long playerId)
        {
            group.Members.Remove(playerId);
            GroupRemovalResult result = new GroupRemovalResult();
            if (group.Members.Count <= 1)
            {
                result.Disbanded = true;
                result.RemainingMembers.AddRange(group.Members.Keys);
                group.Members.Clear();
                return result;
            }
            if (!group.Members.ContainsKey(group.Leader))
            {
                foreach (long member in group.Members.Keys)
                {
                    group.Leader = member;
                    break;
                }
            }
            result.NewLeader = group.Leader;
            result.RemainingMembers.AddRange(group.Members.Keys);
            return result;
        }
    }
}
