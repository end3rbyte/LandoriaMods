using System;
using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal sealed class GroupAcceptanceResult
    {
        internal GroupAcceptanceResult(bool accepted, SocialGroup group, string message)
        {
            Accepted = accepted;
            Group = group;
            Message = message;
        }

        internal bool Accepted { get; }
        internal SocialGroup Group { get; }
        internal string Message { get; }
    }

    internal static class GroupAcceptancePolicy
    {
        internal static GroupAcceptanceResult Accept(long playerId, string playerName,
            string inviterText, IDictionary<long, long> invitations,
            Func<long, SocialGroup> getOrCreateGroup, IDictionary<long, int> playerGroups)
        {
            if (!long.TryParse(inviterText, out long inviter) ||
                !invitations.TryGetValue(playerId, out long expected) || expected != inviter)
            {
                return Reject("That group invitation is no longer valid.");
            }

            SocialGroup group = getOrCreateGroup(inviter);
            if (group == null || group.Members.Count >= SocialGroup.MaximumSize)
            {
                return Reject("That group is no longer available.");
            }

            group.AddMember(playerId, playerName);
            playerGroups[playerId] = group.Id;
            invitations.Remove(playerId);
            return new GroupAcceptanceResult(true, group, null);
        }

        private static GroupAcceptanceResult Reject(string message)
        {
            return new GroupAcceptanceResult(false, null, message);
        }
    }
}
