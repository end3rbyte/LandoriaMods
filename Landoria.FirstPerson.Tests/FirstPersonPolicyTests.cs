using Xunit;

namespace Landoria.FirstPerson.Tests;

public sealed class FirstPersonPolicyTests
{
    [Theory]
    [InlineData("fov", 2, true, 90f, true)]
    [InlineData("fov", 1, false, 0f, false)]
    [InlineData("fov", 2, true, 5f, false)]
    [InlineData("firstperson", 2, true, 90f, false)]
    public void IdentifiesValidFieldOfViewChanges(
        string command, int argumentCount, bool parsed, float fieldOfView, bool expected)
    {
        Assert.Equal(expected, FirstPersonPolicy.ShouldPersistFieldOfView(
            command, argumentCount, parsed, fieldOfView));
    }

    [Theory]
    [InlineData("fov", 2, "reset", true)]
    [InlineData("fov", 2, "RESET", true)]
    [InlineData("fov", 1, null, false)]
    [InlineData("fov", 2, "65", false)]
    [InlineData("firstperson", 2, "reset", false)]
    public void IdentifiesFieldOfViewReset(
        string command, int argumentCount, string value, bool expected)
    {
        Assert.Equal(expected, FirstPersonPolicy.ShouldResetFieldOfView(
            command, argumentCount, value));
    }

    [Theory]
    [InlineData(90f, 90f)]
    [InlineData(100f, 90f)]
    [InlineData(120f, 90f)]
    public void CapsFieldOfViewAtNinety(float value, float expected)
    {
        Assert.Equal(expected, FirstPersonPolicy.ClampFieldOfView(value));
    }

    [Fact]
    public void HidesLivingLocalPlayerAtMinimumDistance()
    {
        Assert.True(FirstPersonPolicy.ShouldApplyFirstPerson(true, true, false, false, 0f));
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
        Assert.False(FirstPersonPolicy.ShouldApplyFirstPerson(
            enabled, hasPlayer, isDead, isFreeFly, distance));
    }
}
