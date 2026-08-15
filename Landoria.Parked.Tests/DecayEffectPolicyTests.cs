using Xunit;
using Landoria.Socialize;

namespace Landoria.Parked;

public sealed class DecayEffectPolicyTests
{
    // Verifies rain damages a building while its creator is connected.
    [Fact]
    public void RainDamagesBuildingWhenCreatorIsConnected()
    {
        float activity = Activity(creator: 1, online: new[] { 1L }, group: null);

        Assert.True(DecayEffectPolicy.ShouldApplyRainDamage(true, activity));
    }

    // Verifies rain does not damage a building while its creator is disconnected.
    [Fact]
    public void RainDoesNotDamageBuildingWhenCreatorIsDisconnected()
    {
        float activity = Activity(creator: 1, online: Array.Empty<long>(), group: null);

        Assert.False(DecayEffectPolicy.ShouldApplyRainDamage(true, activity));
    }

    // Verifies an online group member keeps rain damage active for an offline creator.
    [Fact]
    public void RainDamagesBuildingWhenGroupMemberIsConnected()
    {
        float activity = Activity(creator: 1, online: new[] { 2L }, group: Group(1, 2));

        Assert.True(DecayEffectPolicy.ShouldApplyRainDamage(true, activity));
    }

    // Verifies non-rain damage remains enabled regardless of creator activity.
    [Fact]
    public void NonRainDamageIsNeverPausedByDecayProtection()
    {
        Assert.True(DecayEffectPolicy.ShouldApplyRainDamage(false, 0f));
    }

    // Verifies fires and torches consume fuel while their creator is connected.
    [Theory]
    [InlineData("fire")]
    [InlineData("torch")]
    public void FuelIsConsumedWhenCreatorIsConnected(string pieceType)
    {
        float activity = Activity(creator: 1, online: new[] { 1L }, group: null);

        Assert.False(DecayEffectPolicy.ShouldPauseFuel(false, activity), pieceType);
    }

    // Verifies fires and torches pause fuel while their creator is disconnected.
    [Theory]
    [InlineData("fire")]
    [InlineData("torch")]
    public void FuelIsPausedWhenCreatorIsDisconnected(string pieceType)
    {
        float activity = Activity(creator: 1, online: Array.Empty<long>(), group: null);

        Assert.True(DecayEffectPolicy.ShouldPauseFuel(false, activity), pieceType);
    }

    // Verifies an online group member keeps fires and torches consuming fuel.
    [Theory]
    [InlineData("fire")]
    [InlineData("torch")]
    public void FuelIsConsumedWhenGroupMemberIsConnected(string pieceType)
    {
        float activity = Activity(creator: 1, online: new[] { 2L }, group: Group(1, 2));

        Assert.False(DecayEffectPolicy.ShouldPauseFuel(false, activity), pieceType);
    }

    // Verifies the first fireplace update initializes without consuming fuel.
    [Fact]
    public void FirstFireplaceUpdatePausesFuelConsumption()
    {
        Assert.True(DecayEffectPolicy.ShouldPauseFuel(true, 1f));
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
