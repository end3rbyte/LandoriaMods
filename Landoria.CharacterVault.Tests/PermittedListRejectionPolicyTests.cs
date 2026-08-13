using Xunit;

namespace Landoria.CharacterVault;

public sealed class PermittedListRejectionPolicyTests
{
    // Verifies the permitted-list message shown for an unauthorized new character.
    [Fact]
    public void UnauthorizedNewCharacterReceivesSteamAccountMessage()
    {
        string message = PermittedListRejectionPolicy.MessageFor(isNewCharacter: true);

        Assert.Equal("Steam account not registered for this server.", message);
    }

    // Verifies the permitted-list message shown for an unauthorized existing character.
    [Fact]
    public void UnauthorizedExistingCharacterReceivesSteamAccountMessage()
    {
        string message = PermittedListRejectionPolicy.MessageFor(isNewCharacter: false);

        Assert.Equal("Steam account not registered for this server.", message);
    }
}
