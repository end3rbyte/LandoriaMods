using Xunit;

namespace Landoria.DecayControl;

public sealed class DecayEffectPolicyTests
{
    [Fact]
    public void DefaultModeKeepsVanillaRainWear()
    {
        Assert.True(ShouldWear(DecayControlMode.Default, creatorOnline: false));
    }

    [Fact]
    public void PlayerOnlineModeAppliesRainWearWhileCreatorIsOnline()
    {
        Assert.True(ShouldWear(DecayControlMode.PlayerOnline, creatorOnline: true));
    }

    [Fact]
    public void PlayerOnlineModePausesRainWearWhileCreatorIsOffline()
    {
        Assert.False(ShouldWear(DecayControlMode.PlayerOnline, creatorOnline: false));
    }

    [Fact]
    public void DisabledModeStopsRainWear()
    {
        Assert.False(ShouldWear(DecayControlMode.Disabled, creatorOnline: true));
    }

    [Fact]
    public void NaturalPiecesKeepVanillaRainWear()
    {
        Assert.True(DecayEffectPolicy.ShouldApplyEnvironmentalWear(
            true, false, DecayControlMode.Disabled, 0f));
    }

    [Fact]
    public void NonRainDamageIsNeverPaused()
    {
        Assert.True(DecayEffectPolicy.ShouldApplyEnvironmentalWear(
            false, true, DecayControlMode.Disabled, 0f));
    }

    [Fact]
    public void DefaultModeKeepsVanillaFuelConsumption()
    {
        Assert.False(ShouldPauseFuel(DecayControlMode.Default, false));
    }

    [Fact]
    public void PlayerOnlineModeConsumesFuelWhileCreatorIsOnline()
    {
        Assert.False(ShouldPauseFuel(DecayControlMode.PlayerOnline, true));
    }

    [Fact]
    public void PlayerOnlineModePausesFuelWhileCreatorIsOffline()
    {
        Assert.True(ShouldPauseFuel(DecayControlMode.PlayerOnline, false));
    }

    [Fact]
    public void DisabledModeStopsFuelConsumption()
    {
        Assert.True(ShouldPauseFuel(DecayControlMode.Disabled, true));
    }

    [Fact]
    public void NaturalFireplacesKeepVanillaFuelConsumption()
    {
        Assert.False(DecayEffectPolicy.ShouldPauseFuel(
            false, false, DecayControlMode.Disabled, 0f));
    }

    [Fact]
    public void FirstUpdatePausesCatchUpOnlyInPlayerOnlineMode()
    {
        Assert.True(DecayEffectPolicy.ShouldPauseFuel(
            true, true, DecayControlMode.PlayerOnline, 1f));
        Assert.False(DecayEffectPolicy.ShouldPauseFuel(
            true, true, DecayControlMode.Default, 1f));
    }

    private static bool ShouldWear(DecayControlMode mode, bool creatorOnline)
    {
        return DecayEffectPolicy.ShouldApplyEnvironmentalWear(
            true, true, mode, Activity(creatorOnline));
    }

    private static bool ShouldPauseFuel(DecayControlMode mode, bool creatorOnline)
    {
        return DecayEffectPolicy.ShouldPauseFuel(
            true, false, mode, Activity(creatorOnline));
    }

    private static float Activity(bool creatorOnline)
    {
        return CreatorActivityPolicy.IsCreatorActive(
            1, creatorOnline ? new HashSet<long> { 1 } : new HashSet<long>()) ? 1f : 0f;
    }
}
