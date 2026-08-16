using Xunit;

namespace Landoria.Socialize;

public sealed class GroupLifecyclePolicyTests
{
    // Verifies that a group is disbanded when a departure leaves only one member.
    [Fact]
    public void DepartureDisbandsGroupWithOneRemainingMember()
    {
        SocialGroup group = Group(leader: 1, (1, "Leader"), (2, "Member"));

        GroupRemovalResult result = GroupLifecyclePolicy.Remove(group, 2);

        Assert.True(result.Disbanded);
        Assert.Equal([1L], result.RemainingMembers);
        Assert.Empty(group.Members);
    }

    // Verifies that removing a non-leader preserves the group and its leader.
    [Fact]
    public void RemovingMemberPreservesLeader()
    {
        SocialGroup group = Group(leader: 1, (1, "Leader"), (2, "First"), (3, "Second"));

        GroupRemovalResult result = GroupLifecyclePolicy.Remove(group, 3);

        Assert.False(result.Disbanded);
        Assert.Equal(1, result.NewLeader);
        Assert.Equal([1L, 2L], result.RemainingMembers);
    }

    // Verifies that a departing leader transfers leadership to a remaining member.
    [Fact]
    public void DepartingLeaderTransfersLeadership()
    {
        SocialGroup group = Group(leader: 1, (1, "Leader"), (2, "First"), (3, "Second"));

        GroupRemovalResult result = GroupLifecyclePolicy.Remove(group, 1);

        Assert.False(result.Disbanded);
        Assert.Equal(2, result.NewLeader);
        Assert.Equal(2, group.Leader);
    }

    // Verifies that leadership follows join order rather than player identifier order.
    [Fact]
    public void DepartingLeaderPromotesOldestRemainingMember()
    {
        SocialGroup group = Group(leader: 10, (10, "Leader"), (30, "Oldest"), (20, "Newest"));

        GroupLifecyclePolicy.Remove(group, 10);

        Assert.Equal(30, group.Leader);
    }

    private static SocialGroup Group(long leader, params (long Id, string Name)[] members)
    {
        SocialGroup group = new() { Id = 1, Leader = leader };
        foreach ((long id, string name) in members) group.AddMember(id, name);
        return group;
    }
}
