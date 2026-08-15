using Xunit;

namespace Landoria.Socialize;

public sealed class SocializeArgumentPolicyTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MissingSwitchUsesConfiguredValue(bool configured)
    {
        Assert.Equal(configured, SocializeArgumentPolicy.Resolve(
            System.Array.Empty<string>(), "--setting", configured));
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("FALSE", false)]
    public void SwitchOverridesConfiguredValue(string value, bool expected)
    {
        Assert.Equal(expected, SocializeArgumentPolicy.Resolve(
            new[] { "--setting", value }, "--setting", !expected));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    public void InvalidValueIsRejected(string value)
    {
        Assert.Throws<System.InvalidOperationException>(() =>
            SocializeArgumentPolicy.Resolve(
                new[] { "--setting", value }, "--setting", true));
    }

    [Fact]
    public void MissingValueIsRejected()
    {
        Assert.Throws<System.InvalidOperationException>(() =>
            SocializeArgumentPolicy.Resolve(
                new[] { "--setting" }, "--setting", true));
    }

    [Fact]
    public void DuplicateSwitchIsRejected()
    {
        Assert.Throws<System.InvalidOperationException>(() =>
            SocializeArgumentPolicy.Resolve(
                new[] { "--setting", "true", "--setting", "false" },
                "--setting", true));
    }
}
