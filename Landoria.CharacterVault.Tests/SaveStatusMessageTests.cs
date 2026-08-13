using Xunit;

namespace Landoria.CharacterVault;

public sealed class SaveStatusMessageTests
{
    // Verifies the exact minimap messages used by the character-save lifecycle.
    [Fact]
    public void CharacterSaveUsesExpectedMinimapMessages()
    {
        Assert.Equal("Saving character...", SaveStatusMessages.Saving);
        Assert.Equal("Saving character......", SaveStatusMessages.Accepted);
        Assert.Equal("Character saved", SaveStatusMessages.Saved);
        Assert.Equal("Failed", SaveStatusMessages.Failed);
    }
}
