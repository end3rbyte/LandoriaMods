using System;
using Xunit;

namespace Landoria.DecayControl;

public sealed class DecayControlServerConfigurationTests
{
    [Fact]
    public void DefaultsPreserveVanillaBehavior()
    {
        DecayControlServerConfiguration configuration =
            DecayControlServerConfiguration.FromArguments(Array.Empty<string>());

        Assert.Equal(DecayControlMode.Default, configuration.FuelConsumption);
        Assert.Equal(DecayControlMode.Default, configuration.EnvironmentalBuildingWear);
    }

    [Fact]
    public void ResolvesConfiguredModes()
    {
        DecayControlServerConfiguration configuration =
            DecayControlServerConfiguration.FromArguments(new[]
            {
                "--decay-control-fuel-consumption", "player-online",
                "--decay-control-environmental-building-wear", "disabled"
            });

        Assert.Equal(DecayControlMode.PlayerOnline, configuration.FuelConsumption);
        Assert.Equal(DecayControlMode.Disabled, configuration.EnvironmentalBuildingWear);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("true")]
    public void RejectsInvalidModes(string value)
    {
        Assert.Throws<InvalidOperationException>(() =>
            DecayControlServerConfiguration.FromArguments(new[]
            {
                "--decay-control-fuel-consumption", value
            }));
    }
}
