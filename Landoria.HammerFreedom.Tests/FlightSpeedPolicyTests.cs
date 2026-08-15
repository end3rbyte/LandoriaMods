using Xunit;

namespace Landoria.HammerFreedom;

public sealed class FlightSpeedPolicyTests
{
    [Theory]
    [InlineData(false, 4f)]
    [InlineData(true, 7f)]
    public void MaximumSpeedMatchesSprintState(bool sprinting, float expected)
    {
        Assert.Equal(expected, FlightSpeedPolicy.ResolveMaximumSpeed(sprinting));
    }

    [Theory]
    [InlineData(3f, false, 1f)]
    [InlineData(4f, false, 1f)]
    [InlineData(8f, false, 0.5f)]
    [InlineData(14f, true, 0.5f)]
    public void ScaleOnlyLimitsSpeedAboveMaximum(
        float currentSpeed, bool sprinting, float expected)
    {
        Assert.Equal(expected, FlightSpeedPolicy.ResolveScale(currentSpeed, sprinting));
    }
}
