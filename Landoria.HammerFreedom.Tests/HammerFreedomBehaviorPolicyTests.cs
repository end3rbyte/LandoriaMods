using Xunit;

namespace Landoria.HammerFreedom;

public sealed class HammerFreedomBehaviorPolicyTests
{
    // Verifies that authorized fall damage is cancelled for the local player.
    [Fact]
    public void AuthorizedLocalPlayerFallDamageIsCancelled()
    {
        Assert.False(HammerFreedomBehaviorPolicy.ShouldApplyDamage(true, true, true));
    }

    // Verifies that unrelated damage and remote player damage remain unchanged.
    [Theory]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public void DamageWithoutApplicableAuthorizationIsApplied(
        bool localPlayer, bool fallDamage, bool authorized)
    {
        Assert.True(HammerFreedomBehaviorPolicy.ShouldApplyDamage(
            localPlayer, fallDamage, authorized));
    }

    // Verifies that every local stamina cost is cancelled when authorized.
    [Fact]
    public void AuthorizedLocalPlayerStaminaConsumptionIsCancelled()
    {
        Assert.False(HammerFreedomBehaviorPolicy.ShouldConsumeStamina(true, true));
    }

    // Verifies that denied and non-local stamina consumption remains unchanged.
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void StaminaWithoutApplicableAuthorizationIsConsumed(
        bool localPlayer, bool authorized)
    {
        Assert.True(HammerFreedomBehaviorPolicy.ShouldConsumeStamina(localPlayer, authorized));
    }

    // Verifies that durability is preserved only for an authorized local player.
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public void DurabilityProtectionRequiresLocalAuthorization(
        bool localPlayer, bool authorized, bool expected)
    {
        Assert.Equal(expected, HammerFreedomBehaviorPolicy.ShouldPreserveDurability(
            localPlayer, authorized));
    }

    // Verifies that only an authorized free-build check inside recovery is ignored.
    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, false, false)]
    public void FreeBuildKeyOverrideRequiresAuthorizedRecoveryScope(
        bool scope, bool authorized, bool freeBuildKey, bool expected)
    {
        Assert.Equal(expected, HammerFreedomBehaviorPolicy.ShouldIgnoreFreeBuildKey(
            scope, authorized, freeBuildKey));
    }
}
