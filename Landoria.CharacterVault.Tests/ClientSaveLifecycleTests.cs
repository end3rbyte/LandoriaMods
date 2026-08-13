using Xunit;

namespace Landoria.CharacterVault;

public sealed class ClientSaveLifecycleTests
{
    [Fact]
    public void InactiveClientCannotUpload()
    {
        ClientSaveLifecycle lifecycle = new();

        Assert.False(lifecycle.IsActive);
        Assert.False(lifecycle.CanUpload);
        Assert.False(lifecycle.RecordSpawn(isLocalPlayer: true));
        Assert.False(lifecycle.CanUpload);
    }

    [Fact]
    public void ExistingCharacterCannotUploadBeforeLocalSpawn()
    {
        ClientSaveLifecycle lifecycle = new();

        lifecycle.ActivateExisting();

        Assert.True(lifecycle.IsActive);
        Assert.False(lifecycle.CanUpload);
    }

    [Fact]
    public void RemoteSpawnDoesNotEnableUploads()
    {
        ClientSaveLifecycle lifecycle = new();
        lifecycle.ActivateExisting();

        bool shouldSaveEnrollment = lifecycle.RecordSpawn(isLocalPlayer: false);

        Assert.False(shouldSaveEnrollment);
        Assert.False(lifecycle.CanUpload);
    }

    [Fact]
    public void ExistingCharacterCanUploadAfterLocalSpawn()
    {
        ClientSaveLifecycle lifecycle = new();
        lifecycle.ActivateExisting();

        bool shouldSaveEnrollment = lifecycle.RecordSpawn(isLocalPlayer: true);

        Assert.False(shouldSaveEnrollment);
        Assert.True(lifecycle.CanUpload);
    }

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
