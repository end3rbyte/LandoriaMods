using Xunit;

namespace Landoria.CharacterVault;

public sealed class CharacterVaultArgumentPolicyTests
{
    [Fact]
    public void MissingSwitchAllowsMultipleCharacters()
    {
        Assert.True(CharacterVaultArgumentPolicy.ResolveAllowMultiple(
            System.Array.Empty<string>()));
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("FALSE", false)]
    public void SwitchControlsMultipleCharacters(string value, bool expected)
    {
        Assert.Equal(expected, CharacterVaultArgumentPolicy.ResolveAllowMultiple(
            new[] { "--charactervault-allow-multiple-characters", value }));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    public void InvalidValueIsRejected(string value)
    {
        Assert.Throws<System.InvalidOperationException>(() =>
            CharacterVaultArgumentPolicy.ResolveAllowMultiple(new[]
            {
                "--charactervault-allow-multiple-characters", value
            }));
    }
}
