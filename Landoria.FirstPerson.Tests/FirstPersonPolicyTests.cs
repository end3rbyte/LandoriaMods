using Xunit;

namespace Landoria.FirstPerson.Tests;

public sealed class FirstPersonPolicyTests
{
    [Theory]
    [InlineData("True", true)]
    [InlineData("true", true)]
    [InlineData("False", false)]
    [InlineData(null, false)]
    public void ParsesSavedPreference(string value, bool expected)
    {
        Assert.Equal(expected, FirstPersonPolicy.IsPreferenceEnabled(value));
    }

    [Fact]
    public void HidesLivingLocalPlayerAtMinimumDistance()
    {
        Assert.True(FirstPersonPolicy.ShouldHidePlayer(true, true, false, false, 0f));
    }

    [Theory]
    [InlineData(false, true, false, false, 0f)]
    [InlineData(true, false, false, false, 0f)]
    [InlineData(true, true, true, false, 0f)]
    [InlineData(true, true, false, true, 0f)]
    [InlineData(true, true, false, false, 0.5f)]
    public void KeepsPlayerVisibleOutsideFirstPerson(
        bool enabled, bool hasPlayer, bool isDead, bool isFreeFly, float distance)
    {
        Assert.False(FirstPersonPolicy.ShouldHidePlayer(
            enabled, hasPlayer, isDead, isFreeFly, distance));
    }
}
