namespace Landoria.Socialize
{
    internal sealed class GroupDecision
    {
        private GroupDecision(bool allowed, string message)
        {
            Allowed = allowed;
            Message = message;
        }

        internal bool Allowed { get; }
        internal string Message { get; }

        internal static GroupDecision Allow()
        {
            return new GroupDecision(true, null);
        }

        internal static GroupDecision Deny(string message = null)
        {
            return new GroupDecision(false, message);
        }
    }

    internal static class GroupPolicy
    {
        internal static GroupDecision CanInviteTarget(bool targetReady)
        {
            return targetReady
                ? GroupDecision.Allow()
                : GroupDecision.Deny("Player not found or not ready.");
        }

        internal static GroupDecision CanInvite(SocialGroup group, long inviter, long target,
            bool targetAlreadyGrouped)
        {
            if (targetAlreadyGrouped)
            {
                return GroupDecision.Deny("That player is already in a group.");
            }
            if (group != null && group.Leader != inviter)
            {
                return GroupDecision.Deny("Only the group leader can invite players.");
            }
            if (group != null && group.Members.Count >= SocialGroup.MaximumSize)
            {
                return GroupDecision.Deny("Your group is full.");
            }
            return inviter != target ? GroupDecision.Allow() : GroupDecision.Deny();
        }

        internal static GroupDecision CanTargetMember(
            SocialGroup group, long actor, long target, string targetName)
        {
            if (group == null)
            {
                return GroupDecision.Deny("You are not in a group.");
            }
            if (group.Leader != actor)
            {
                return GroupDecision.Deny("Only the group leader can do that.");
            }
            return target != 0L
                ? GroupDecision.Allow()
                : GroupDecision.Deny("Player not found in your group: " + targetName);
        }

        internal static GroupDecision CanRemove(long actor, long target)
        {
            return target != actor
                ? GroupDecision.Allow()
                : GroupDecision.Deny("You cannot remove yourself.");
        }

        internal static GroupDecision CanPromote(long actor, long target)
        {
            return target != actor
                ? GroupDecision.Allow()
                : GroupDecision.Deny("You are already group leader.");
        }
    }
}
