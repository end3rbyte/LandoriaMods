using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal static class MapSharingPolicy
    {
        internal static bool CanSendPublicPing(bool restricted, bool isInGroup) =>
            !restricted || isInGroup;

        internal static bool GetPublicPosition(bool restricted, bool requested) =>
            restricted ? false : requested;

        internal static bool ShouldAddGroupMember(
            long playerId, long localPlayerId, ISet<long> visiblePlayerIds)
        {
            return playerId != localPlayerId && !visiblePlayerIds.Contains(playerId);
        }

        internal static List<long> GetGroupPingRecipients(
            long localPlayerId, ISet<long> groupMembers, IEnumerable<long> connectedPlayers)
        {
            List<long> recipients = new List<long>();
            if (localPlayerId == 0L || !groupMembers.Contains(localPlayerId)) return recipients;
            recipients.Add(localPlayerId);
            foreach (long playerId in connectedPlayers)
            {
                if (playerId != localPlayerId && groupMembers.Contains(playerId) &&
                    !recipients.Contains(playerId))
                {
                    recipients.Add(playerId);
                }
            }
            return recipients;
        }
    }
}
