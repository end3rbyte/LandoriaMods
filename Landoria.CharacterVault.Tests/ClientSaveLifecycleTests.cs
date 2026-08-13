using Xunit;

namespace Landoria.CharacterVault;

public sealed class ClientSaveLifecycleTests
{
    // Verifies that a disconnected or inactive client cannot upload a profile.
    [Fact]
    public void InactiveClientCannotUpload()
    {
        ClientSaveLifecycle lifecycle = new();

        Assert.False(lifecycle.IsActive);
        Assert.False(lifecycle.CanUpload);
        Assert.False(lifecycle.RecordSpawn(isLocalPlayer: true));
        Assert.False(lifecycle.CanUpload);
    }

    // Verifies that loading an existing profile does not permit uploads before local spawn.
    [Fact]
    public void ExistingCharacterCannotUploadBeforeLocalSpawn()
    {
        ClientSaveLifecycle lifecycle = new();

        lifecycle.ActivateExisting();

        Assert.True(lifecycle.IsActive);
        Assert.False(lifecycle.CanUpload);
    }

    // Verifies that another player's spawn cannot unlock the local profile upload gate.
    [Fact]
    public void RemoteSpawnDoesNotEnableUploads()
    {
        ClientSaveLifecycle lifecycle = new();
        lifecycle.ActivateExisting();

        bool shouldSaveEnrollment = lifecycle.RecordSpawn(isLocalPlayer: false);

        Assert.False(shouldSaveEnrollment);
        Assert.False(lifecycle.CanUpload);
    }

    // Verifies that the local spawn unlocks uploads for an existing server profile.
    [Fact]
    public void ExistingCharacterCanUploadAfterLocalSpawn()
    {
        ClientSaveLifecycle lifecycle = new();
        lifecycle.ActivateExisting();

        bool shouldSaveEnrollment = lifecycle.RecordSpawn(isLocalPlayer: true);

        Assert.False(shouldSaveEnrollment);
        Assert.True(lifecycle.CanUpload);
    }

    // Verifies that a new character requests exactly one enrollment save after local spawn.
    [Fact]
    public void NewCharacterRequestsOneEnrollmentSaveAfterLocalSpawn()
    {
        ClientSaveLifecycle lifecycle = new();
        lifecycle.BeginEnrollment();

        Assert.False(lifecycle.CanUpload);
        Assert.False(lifecycle.RecordSpawn(isLocalPlayer: false));
        Assert.False(lifecycle.CanUpload);
        Assert.True(lifecycle.RecordSpawn(isLocalPlayer: true));
        Assert.True(lifecycle.CanUpload);
        Assert.False(lifecycle.RecordSpawn(isLocalPlayer: true));
    }

    // Verifies that reconnecting closes the upload gate until the next local spawn.
    [Fact]
    public void ResetBlocksUploadsUntilTheNextLocalSpawn()
    {
        ClientSaveLifecycle lifecycle = new();
        lifecycle.ActivateExisting();
        lifecycle.RecordSpawn(isLocalPlayer: true);

        lifecycle.Reset();
        lifecycle.ActivateExisting();

        Assert.False(lifecycle.CanUpload);
    }

    // Verifies that disconnecting cancels an enrollment save that has not started.
    [Fact]
    public void ResetCancelsPendingEnrollmentSave()
    {
        ClientSaveLifecycle lifecycle = new();
        lifecycle.BeginEnrollment();

        lifecycle.Reset();
        lifecycle.ActivateExisting();

        Assert.False(lifecycle.RecordSpawn(isLocalPlayer: true));
        Assert.True(lifecycle.CanUpload);
    }

    // Verifies that duplicate existing-profile activation cannot mimic enrollment.
    [Fact]
    public void RepeatedActivationDoesNotCreateEnrollmentSave()
    {
        ClientSaveLifecycle lifecycle = new();

        lifecycle.ActivateExisting();
        lifecycle.ActivateExisting();

        Assert.False(lifecycle.RecordSpawn(isLocalPlayer: true));
        Assert.True(lifecycle.CanUpload);
    }
}
