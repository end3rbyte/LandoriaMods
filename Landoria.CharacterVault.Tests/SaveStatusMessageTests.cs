using Xunit;

namespace Landoria.CharacterVault;

public sealed class SaveStatusMessageTests
{
    // Verifies every minimap message shown during a successful character save.
    [Fact]
    public void SuccessfulSaveShowsExpectedMessageSequence()
    {
        SaveStatusLifecycle lifecycle = new();
        lifecycle.Begin("save-1", false, SaveStatusMessages.Saving);
        Assert.Equal("Saving character...", lifecycle.Message);

        lifecycle.Begin("save-1", true, SaveStatusMessages.Accepted);
        Assert.Equal("Saving character......", lifecycle.Message);
        Assert.True(lifecycle.CanCommit("save-1"));

        lifecycle.Begin("save-1", false, SaveStatusMessages.Saved);
        Assert.Equal("Character saved", lifecycle.Message);
    }

    // Verifies that a missing initial acknowledgement never shows an accepted or saved message.
    [Fact]
    public void MissingAcknowledgementLeavesSavingMessageUntilItExpires()
    {
        SaveStatusLifecycle lifecycle = new();
        int version = lifecycle.Begin("save-2", false, SaveStatusMessages.Saving);

        Assert.Equal("Saving character...", lifecycle.Message);
        Assert.False(lifecycle.CanFail("save-2", version));

        lifecycle.Clear();
        Assert.False(lifecycle.Visible);
    }

    // Verifies that a missing commit confirmation produces the minimap failure message.
    [Fact]
    public void MissingCommitConfirmationShowsFailedMessage()
    {
        SaveStatusLifecycle lifecycle = new();
        int version = lifecycle.Begin("save-3", true, SaveStatusMessages.Accepted);

        Assert.True(lifecycle.CanFail("save-3", version));
        lifecycle.Begin("save-3", false, SaveStatusMessages.Failed);

        Assert.Equal("Failed", lifecycle.Message);
    }
}
