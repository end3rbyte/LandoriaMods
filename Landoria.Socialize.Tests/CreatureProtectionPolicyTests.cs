using Xunit;

namespace Landoria.Socialize;

public sealed class CreatureProtectionPolicyTests
{
    // Verifies monsters can target a building while its creator is connected.
    [Fact]
    public void MonsterTargetsBuildingWhenCreatorIsConnected()
    {
        float activity = Activity(creator: 1, online: new[] { 1L }, group: null);

        Assert.True(CreatureProtectionPolicy.CanTarget(true, activity));
    }

    // Verifies monsters ignore a building while its creator is disconnected.
    [Fact]
    public void MonsterIgnoresBuildingWhenCreatorIsDisconnected()
    {
        float activity = Activity(creator: 1, online: Array.Empty<long>(), group: null);

        Assert.False(CreatureProtectionPolicy.CanTarget(true, activity));
    }

    // Verifies an online group member keeps an offline creator's building targetable.
    [Fact]
    public void MonsterTargetsBuildingWhenGroupMemberIsConnected()
    {
        float activity = Activity(creator: 1, online: new[] { 2L }, group: Group(1, 2));

        Assert.True(CreatureProtectionPolicy.CanTarget(true, activity));
    }

    // Verifies Socialize never makes a vanilla-ineligible target aggressive.
    [Fact]
    public void VanillaIneligibleTargetRemainsIgnored()
    {
        Assert.False(CreatureProtectionPolicy.CanTarget(false, 1f));
    }

    // Verifies monsters can damage a building while its creator is connected.
    [Fact]
    public void MonsterDamagesBuildingWhenCreatorIsConnected()
    {
        float activity = Activity(creator: 1, online: new[] { 1L }, group: null);

        Assert.True(CreatureProtectionPolicy.CanDamageBuilding(activity));
    }

    // Verifies monsters cannot damage a building while its creator is disconnected.
    [Fact]
    public void MonsterCannotDamageBuildingWhenCreatorIsDisconnected()
    {
        float activity = Activity(creator: 1, online: Array.Empty<long>(), group: null);

        Assert.False(CreatureProtectionPolicy.CanDamageBuilding(activity));
    }

    // Verifies an online group member enables damage to an offline creator's building.
    [Fact]
    public void MonsterDamagesBuildingWhenGroupMemberIsConnected()
    {
        float activity = Activity(creator: 1, online: new[] { 2L }, group: Group(1, 2));

        Assert.True(CreatureProtectionPolicy.CanDamageBuilding(activity));
    }

    private static float Activity(long creator, IEnumerable<long> online, SocialGroup group)
    {
        return CreatorActivityPolicy.IsCreatorActive(
            creator, new HashSet<long>(online), group) ? 1f : 0f;
    }

    private static SocialGroup Group(params long[] members)
    {
        SocialGroup group = new() { Id = 1, Leader = members[0] };
        foreach (long member in members)
        {
            group.Members[member] = "Player" + member;
        }
        return group;
    }
}
