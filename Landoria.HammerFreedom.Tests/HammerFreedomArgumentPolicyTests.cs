using Xunit;

namespace Landoria.HammerFreedom;

public sealed class HammerFreedomArgumentPolicyTests
{
    // Verifies that an omitted server switch disables its capability by default.
    [Fact]
    public void MissingSwitchDefaultsToDisabled()
    {
        bool enabled = HammerFreedomArgumentPolicy.Resolve(
            Array.Empty<string>(), "--hammerfreedom-fly", out bool valid);

        Assert.True(valid);
        Assert.False(enabled);
    }

    // Verifies that every HammerFreedom switch can independently disable its capability.
    [Theory]
    [InlineData("--hammerfreedom-fly")]
    [InlineData("--hammerfreedom-fall-damage-immunity")]
    [InlineData("--hammerfreedom-unlimited-stamina")]
    [InlineData("--hammerfreedom-no-durability-loss")]
    [InlineData("--hammerfreedom-recover-build-materials")]
    public void ExplicitFalseDisablesCapability(string name)
    {
        bool enabled = HammerFreedomArgumentPolicy.Resolve(
            new[] { name.ToUpperInvariant(), "false" }, name, out bool valid);

        Assert.True(valid);
        Assert.False(enabled);
    }

    // Verifies that an invalid switch value is reported and falls back to disabled.
    [Fact]
    public void InvalidValueFallsBackToDisabled()
    {
        bool enabled = HammerFreedomArgumentPolicy.Resolve(
            new[] { "--hammerfreedom-fly", "invalid" },
            "--hammerfreedom-fly", out bool valid);

        Assert.False(valid);
        Assert.False(enabled);
    }
}
