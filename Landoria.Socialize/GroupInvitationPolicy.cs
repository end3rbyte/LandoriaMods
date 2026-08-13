using System.Collections.Generic;

namespace Landoria.Socialize
{
    internal static class GroupInvitationPolicy
    {
        internal static GroupDecision TryInvite(SocialGroup group, long inviter, long target,
            bool targetAlreadyGrouped, IDictionary<long, long> invitations)
        {
            GroupDecision decision = GroupPolicy.CanInvite(
                group, inviter, target, targetAlreadyGrouped);
            if (decision.Allowed)
            {
                invitations[target] = inviter;
            }
            return decision;
        }
    }
}
