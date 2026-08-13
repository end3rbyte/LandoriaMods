using Xunit;

namespace Landoria.Socialize;

public sealed class GroupPolicyTests
{
    // Verifies that an ungrouped player can invite another player.
    [Fact]
    public void UngroupedPlayerCanInviteAnotherPlayer()
    {
        GroupDecision decision = GroupPolicy.CanInvite(null, 1, 2, false);

        Assert.True(decision.Allowed);
        Assert.Null(decision.Message);
    }

    // Verifies every reason that blocks a group invitation.
    [Theory]
    [InlineData("target-grouped", "That player is already in a group.")]
    [InlineData("not-leader", "Only the group leader can invite players.")]
    [InlineData("full", "Your group is full.")]
    public void InvalidInvitationIsRejected(string scenario, string message)
    {
        SocialGroup group = Group(leader: scenario == "not-leader" ? 9 : 1,
            memberCount: scenario == "full" ? SocialGroup.MaximumSize : 2);

        GroupDecision decision = GroupPolicy.CanInvite(
            group, 1, 2, scenario == "target-grouped");

        Assert.False(decision.Allowed);
        Assert.Equal(message, decision.Message);
    }

    // Verifies that inviting oneself is ignored without a misleading player error.
    [Fact]
    public void PlayerCannotInviteSelf()
    {
        GroupDecision decision = GroupPolicy.CanInvite(null, 1, 1, false);

        Assert.False(decision.Allowed);
        Assert.Null(decision.Message);
    }

    // Verifies leader-action validation for membership, leadership, and target lookup.
    [Theory]
    [InlineData("no-group", "You are not in a group.")]
    [InlineData("not-leader", "Only the group leader can do that.")]
    [InlineData("missing-target", "Player not found in your group: Alice")]
    public void InvalidLeaderActionIsRejected(string scenario, string message)
    {
        SocialGroup group = scenario == "no-group" ? null : Group(
            leader: scenario == "not-leader" ? 9 : 1, memberCount: 2);
        long target = scenario == "missing-target" ? 0 : 2;

        GroupDecision decision = GroupPolicy.CanTargetMember(group, 1, target, "Alice");

        Assert.False(decision.Allowed);
        Assert.Equal(message, decision.Message);
    }

    // Verifies that a leader can target an existing member.
    [Fact]
    public void LeaderCanTargetExistingMember()
    {
        Assert.True(GroupPolicy.CanTargetMember(Group(1, 2), 1, 2, "Alice").Allowed);
    }

    // Verifies that leaders cannot remove themselves or promote themselves again.
    [Fact]
    public void SelfActionsAreRejected()
    {
        Assert.Equal("You cannot remove yourself.", GroupPolicy.CanRemove(1, 1).Message);
        Assert.Equal("You are already group leader.", GroupPolicy.CanPromote(1, 1).Message);
        Assert.True(GroupPolicy.CanRemove(1, 2).Allowed);
        Assert.True(GroupPolicy.CanPromote(1, 2).Allowed);
    }

    private static SocialGroup Group(long leader, int memberCount)
    {
        SocialGroup group = new() { Id = 1, Leader = leader };
        for (int id = 1; id <= memberCount; id++)
        {
            group.Members[id] = "Player" + id;
        }
        return group;
    }
}
