using Xunit;

namespace Landoria.FirstPerson.Tests;

public sealed class FirstPersonPolicyTests
{
    [Theory]
    [InlineData("fov", 2, true, 85f, true)]
    [InlineData("fov", 2, true, 86f, false)]
    [InlineData("fov", 1, false, 0f, false)]
    [InlineData("fov", 2, true, 5f, false)]
    [InlineData("firstperson", 2, true, 85f, false)]
    public void IdentifiesValidFieldOfViewChanges(
        string command, int argumentCount, bool parsed, float fieldOfView, bool expected)
    {
        Assert.Equal(expected, FirstPersonPolicy.ShouldPersistFieldOfView(
            command, argumentCount, parsed, fieldOfView));
    }

    [Theory]
    [InlineData("fov", 2, true, 85f, false)]
    [InlineData("fov", 2, true, 86f, true)]
    [InlineData("fov", 1, false, 0f, false)]
    [InlineData("firstperson", 2, true, 86f, false)]
    public void IdentifiesFieldOfViewValuesAboveMaximum(
        string command, int argumentCount, bool parsed, float fieldOfView, bool expected)
    {
        Assert.Equal(expected, FirstPersonPolicy.ShouldRejectFieldOfView(
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
    [InlineData(85f, 85f)]
    [InlineData(90f, 85f)]
    [InlineData(120f, 85f)]
    public void CapsConfiguredFieldOfViewAtEightyFive(float value, float expected)
    {
        Assert.Equal(expected, FirstPersonPolicy.ClampFieldOfView(value));
    }

    [Theory]
    [InlineData(65f, false, 65f)]
    [InlineData(65f, true, 80f)]
    [InlineData(85f, true, 100f)]
    public void AddsFifteenDegreesOnlyInFirstPerson(
        float configured, bool firstPersonActive, float expected)
    {
        Assert.Equal(expected, FirstPersonPolicy.EffectiveFieldOfView(
            configured, firstPersonActive));
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
