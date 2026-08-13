using Xunit;

namespace Landoria.HammerFreedom;

public sealed class HammerFreedomCapabilityPolicyTests
{
    // Verifies that non-Hammer worlds never receive creative capabilities.
    [Fact]
    public void ResolveDeniesEveryCapabilityOutsideHammerWorlds()
    {
        HammerFreedomCapabilities actual = HammerFreedomCapabilityPolicy.Resolve(
            false, true, true, true);

        Assert.Equal(HammerFreedomCapabilities.None, actual);
    }

    // Verifies that each server switch independently controls its capability.
    [Theory]
    [InlineData(true, false, false, 1)]
    [InlineData(false, true, false, 2)]
    [InlineData(false, false, true, 4)]
    public void ResolveGrantsOnlyEnabledCapabilities(
        bool flight, bool fallImmunity, bool stamina, int expected)
    {
        HammerFreedomCapabilities actual = HammerFreedomCapabilityPolicy.Resolve(
            true, flight, fallImmunity, stamina);

        Assert.Equal((HammerFreedomCapabilities)expected, actual);
    }

    // Verifies that a fully enabled Hammer server grants every capability.
    [Fact]
    public void ResolveGrantsAllEnabledCapabilities()
    {
        HammerFreedomCapabilities expected = HammerFreedomCapabilities.Flight |
            HammerFreedomCapabilities.FallDamageImmunity |
            HammerFreedomCapabilities.UnlimitedStamina;

        Assert.Equal(expected, HammerFreedomCapabilityPolicy.Resolve(true, true, true, true));
    }
}
