using System;
using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal static class GroupInfoPolicy
    {
        internal static string Build(SocialGroup group, Func<long, bool> isOnline)
        {
            if (group == null)
            {
                return "You are not in a group.";
            }

            List<string> lines = new List<string> { "Group members:" };
            foreach (KeyValuePair<long, string> member in group.Members)
            {
                string status = isOnline(member.Key) ? "Connected" : "Disconnected";
                string leader = member.Key == group.Leader ? " - Group Leader" : "";
                lines.Add(member.Value + " - " + status + leader);
            }
            return string.Join("\n", lines);
        }
    }
}
