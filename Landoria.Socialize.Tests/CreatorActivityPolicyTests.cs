using Xunit;

namespace Landoria.Socialize;

public sealed class CreatorActivityPolicyTests
{
    // Verifies that online players and all members of their groups are active creators.
    [Fact]
    public void OnlineMemberActivatesEntireGroup()
    {
        SocialGroup activeGroup = Group(1, 2, 3);
        SocialGroup inactiveGroup = Group(4, 5);

        HashSet<long> active = CreatorActivityPolicy.GetActiveCreators(
            new[] { 2L, 9L }, new[] { activeGroup, inactiveGroup });

        Assert.Equal(new HashSet<long> { 1, 2, 3, 9 }, active);
    }

    // Verifies that an online creator is active without a group.
    [Fact]
    public void OnlineCreatorIsActiveWithoutGroup()
    {
        Assert.True(CreatorActivityPolicy.IsCreatorActive(
            1, new HashSet<long> { 1 }, null));
    }

    // Verifies that an offline creator is active while another group member is online.
    [Fact]
    public void OnlineGroupMemberActivatesOfflineCreator()
    {
        Assert.True(CreatorActivityPolicy.IsCreatorActive(
            1, new HashSet<long> { 2 }, Group(1, 2)));
    }

    // Verifies that an offline creator without an online group member is inactive.
    [Fact]
    public void OfflineGroupIsInactive()
    {
        Assert.False(CreatorActivityPolicy.IsCreatorActive(
            1, new HashSet<long> { 9 }, Group(1, 2)));
    }

    private static SocialGroup Group(params long[] members)
    {
        SocialGroup group = new() { Id = (int)members[0], Leader = members[0] };
        foreach (long member in members)
        {
            group.Members[member] = "Player" + member;
        }
        return group;
    }
}
