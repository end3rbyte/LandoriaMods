using Xunit;

namespace Landoria.StructureProtection;

public sealed class StructureProtectionServerConfigurationTests
{
    [Fact]
    public void DefaultsEnableBothProtections()
    {
        StructureProtectionServerConfiguration configuration =
            StructureProtectionServerConfiguration.FromArguments(Array.Empty<string>());

        Assert.True(configuration.CreatureTargetingEnabled);
        Assert.True(configuration.WardPlayerDamageEnabled);
    }

    [Fact]
    public void SwitchesCanDisableProtectionsIndependently()
    {
        StructureProtectionServerConfiguration configuration =
            StructureProtectionServerConfiguration.FromArguments(new[]
            {
                "--structure-protection-creature-targeting", "false",
                "--structure-protection-ward-player-damage", "true"
            });

        Assert.False(configuration.CreatureTargetingEnabled);
        Assert.True(configuration.WardPlayerDamageEnabled);
    }

    [Fact]
    public void InvalidBooleanIsRejected()
    {
        Assert.Throws<InvalidOperationException>(() =>
            StructureProtectionServerConfiguration.FromArguments(new[]
            {
                "--structure-protection-creature-targeting", "enabled"
            }));
    }
}
