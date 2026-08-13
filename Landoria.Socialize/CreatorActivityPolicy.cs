using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal static class CreatorActivityPolicy
    {
        internal static HashSet<long> GetActiveCreators(
            IEnumerable<long> onlinePlayers, IEnumerable<SocialGroup> groups)
        {
            HashSet<long> online = new HashSet<long>(onlinePlayers);
            HashSet<long> active = new HashSet<long>(online);
            foreach (SocialGroup group in groups)
            {
                if (!HasOnlineMember(group, online))
                {
                    continue;
                }
                foreach (long member in group.Members.Keys)
                {
                    active.Add(member);
                }
            }
            return active;
        }

        internal static bool IsCreatorActive(
            long creator, ISet<long> onlinePlayers, SocialGroup group)
        {
            return onlinePlayers.Contains(creator) ||
                   group != null && HasOnlineMember(group, onlinePlayers);
        }

        private static bool HasOnlineMember(SocialGroup group, ISet<long> onlinePlayers)
        {
            foreach (long member in group.Members.Keys)
            {
                if (onlinePlayers.Contains(member))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
