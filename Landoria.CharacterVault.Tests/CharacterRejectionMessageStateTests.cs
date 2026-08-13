using Xunit;

namespace Landoria.CharacterVault;

public sealed class CharacterRejectionMessageStateTests
{
    // Verifies that a permitted-list rejection is exposed unchanged to the client error UI.
    [Fact]
    public void PermittedListRejectionExposesExpectedClientMessage()
    {
        CharacterRejectionMessageState state = new();

        state.Receive(CharacterRejectionMessages.PermittedListDenied);

        Assert.True(state.TryGet(out string message));
        Assert.Equal("Steam account not registered for this server.", message);
    }
}
