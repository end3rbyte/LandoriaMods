using Xunit;

namespace Landoria.HammerFreedom;

public sealed class FlightGroundActionPolicyTests
{
    [Theory]
    [InlineData(true, true, true, false)]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, true)]
    [InlineData(false, true, true, true)]
    public void AppliesGroundActionUnlessAuthorizedLocalPlayerIsFlying(
        bool localPlayer, bool flying, bool authorized, bool expected)
    {
        Assert.Equal(expected,
            FlightGroundActionPolicy.ShouldApply(localPlayer, flying, authorized));
    }
}
