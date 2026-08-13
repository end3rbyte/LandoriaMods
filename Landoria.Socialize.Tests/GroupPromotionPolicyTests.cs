using Xunit;

namespace Landoria.Socialize;

public sealed class GroupPromotionPolicyTests
{
    // Verifies that a non-leader cannot promote another member or change the leader.
    [Fact]
    public void NonLeaderCannotPromoteMember()
    {
        SocialGroup group = Group();

        GroupDecision decision = GroupPromotionPolicy.TryPromote(
            group, actor: 2, target: 3, targetName: "Candidate");

        Assert.False(decision.Allowed);
        Assert.Equal("Only the group leader can do that.", decision.Message);
        Assert.Equal(1, group.Leader);
    }

    // Verifies that the current leader can promote an existing member.
    [Fact]
    public void LeaderCanPromoteMember()
    {
        SocialGroup group = Group();

        GroupDecision decision = GroupPromotionPolicy.TryPromote(
            group, actor: 1, target: 3, targetName: "Candidate");

        Assert.True(decision.Allowed);
        Assert.Equal(3, group.Leader);
    }

    // Verifies that a leader cannot promote an unknown member.
    [Fact]
    public void LeaderCannotPromoteUnknownMember()
    {
        SocialGroup group = Group();

        GroupDecision decision = GroupPromotionPolicy.TryPromote(
            group, actor: 1, target: 0, targetName: "Missing");

        Assert.False(decision.Allowed);
        Assert.Equal("Player not found in your group: Missing", decision.Message);
        Assert.Equal(1, group.Leader);
    }

    private static SocialGroup Group()
    {
        SocialGroup group = new() { Id = 1, Leader = 1 };
        group.Members[1] = "Leader";
        group.Members[2] = "Member";
        group.Members[3] = "Candidate";
        return group;
    }
}
