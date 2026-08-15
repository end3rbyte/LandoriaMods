using Xunit;

namespace Landoria.Parked;

public sealed class CreatureProtectionPolicyTests
{
    [Fact]
    public void MonsterTargetsBuildingWhenCreatorIsConnected()
    {
        Assert.True(CreatureProtectionPolicy.CanTarget(true, Activity(1, 1)));
    }

    [Fact]
    public void MonsterIgnoresBuildingWhenCreatorIsDisconnected()
    {
        Assert.False(CreatureProtectionPolicy.CanTarget(true, Activity(1, 2)));
    }

    [Fact]
    public void VanillaIneligibleTargetRemainsIgnored()
    {
        Assert.False(CreatureProtectionPolicy.CanTarget(false, 1f));
    }

    [Fact]
    public void MonsterDamagesBuildingWhenCreatorIsConnected()
    {
        Assert.True(CreatureProtectionPolicy.CanDamageBuilding(Activity(1, 1)));
    }

    [Fact]
    public void MonsterCannotDamageBuildingWhenCreatorIsDisconnected()
    {
        Assert.False(CreatureProtectionPolicy.CanDamageBuilding(Activity(1, 2)));
    }

    private static float Activity(long creator, params long[] online)
    {
        return CreatorActivityPolicy.IsCreatorActive(
            creator, new HashSet<long>(online)) ? 1f : 0f;
    }
}
