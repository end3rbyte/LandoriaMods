namespace Landoria.Socialize
{
    internal static class GroupPromotionPolicy
    {
        internal static GroupDecision TryPromote(
            SocialGroup group, long actor, long target, string targetName)
        {
            GroupDecision decision = GroupPolicy.CanTargetMember(
                group, actor, target, targetName);
            if (!decision.Allowed)
            {
                return decision;
            }

            decision = GroupPolicy.CanPromote(actor, target);
            if (decision.Allowed)
            {
                group.Leader = target;
            }
            return decision;
        }
    }
}
