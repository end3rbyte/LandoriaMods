using Xunit;

namespace Landoria.HammerFreedom;

public sealed class HammerFreedomArgumentPolicyTests
{
    // Verifies that an omitted server switch enables its capability by default.
    [Fact]
    public void MissingSwitchDefaultsToEnabled()
    {
        bool enabled = HammerFreedomArgumentPolicy.Resolve(
            Array.Empty<string>(), "--hammerfreedom-fly", out bool valid);

        Assert.True(valid);
        Assert.True(enabled);
    }

    // Verifies that every HammerFreedom switch can independently disable its capability.
    [Theory]
    [InlineData("--hammerfreedom-fly")]
    [InlineData("--hammerfreedom-fall-damage-immunity")]
    [InlineData("--hammerfreedom-unlimited-stamina")]
    [InlineData("--hammerfreedom-no-durability-loss")]
    public void ExplicitFalseDisablesCapability(string name)
    {
        bool enabled = HammerFreedomArgumentPolicy.Resolve(
            new[] { name.ToUpperInvariant(), "false" }, name, out bool valid);

        Assert.True(valid);
        Assert.False(enabled);
    }

    // Verifies that an invalid switch value is reported and falls back to enabled.
    [Fact]
    public void InvalidValueFallsBackToEnabled()
    {
        bool enabled = HammerFreedomArgumentPolicy.Resolve(
            new[] { "--hammerfreedom-fly", "invalid" },
            "--hammerfreedom-fly", out bool valid);

        Assert.False(valid);
        Assert.True(enabled);
    }
}
