using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal sealed class PersistedGroup
    {
        internal int Id;
        internal long Leader;
        internal readonly Dictionary<long, string> Members =
            new Dictionary<long, string>();
    }

    internal static class GroupPersistencePolicy
    {
        internal static List<PersistedGroup> Capture(IEnumerable<SocialGroup> groups)
        {
            List<PersistedGroup> result = new List<PersistedGroup>();
            foreach (SocialGroup group in groups)
            {
                PersistedGroup persisted = new PersistedGroup
                {
                    Id = group.Id,
                    Leader = group.Leader
                };
                foreach (KeyValuePair<long, string> member in group.Members)
                {
                    persisted.Members[member.Key] = member.Value;
                }
                result.Add(persisted);
            }
            return result;
        }

        internal static void Restore(
            IEnumerable<PersistedGroup> source,
            IDictionary<int, SocialGroup> groups,
            IDictionary<long, int> playerGroups)
        {
            groups.Clear();
            playerGroups.Clear();
            foreach (PersistedGroup persisted in source)
            {
                SocialGroup group = new SocialGroup
                {
                    Id = persisted.Id,
                    Leader = persisted.Leader
                };
                foreach (KeyValuePair<long, string> member in persisted.Members)
                {
                    group.Members[member.Key] = member.Value;
                    playerGroups[member.Key] = group.Id;
                }
                groups[group.Id] = group;
            }
        }
    }
}
