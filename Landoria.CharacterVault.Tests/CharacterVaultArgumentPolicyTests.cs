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

    [Fact]
    public void MissingStartingItemsSwitchUsesEmptyListText()
    {
        Assert.Equal("", CharacterVaultArgumentPolicy.ResolveStartingItems(
            System.Array.Empty<string>()));
    }

    [Fact]
    public void StartingItemsSwitchReturnsServerValue()
    {
        Assert.Equal("hammer:1,wood:10",
            CharacterVaultArgumentPolicy.ResolveStartingItems(new[]
            {
                "--charactervault-starting-items", "hammer:1,wood:10"
            }));
    }
}
