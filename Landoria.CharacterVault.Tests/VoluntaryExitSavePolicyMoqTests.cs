using Moq;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class VoluntaryExitSavePolicyMoqTests
{
    [Fact]
    public void ExitBeforeSpawnDoesNotRequestServerSave()
    {
        // Exiting before the local player spawns preserves vanilla behavior without an upload.
        Mock<IVoluntaryExitSaveRequest> request = new(MockBehavior.Strict);

        VoluntaryExitSaveAction action = VoluntaryExitSavePolicy.Start(
            playerEnteredWorld: false, savePending: false, request.Object);

        Assert.Equal(VoluntaryExitSaveAction.PassThrough, action);
        request.VerifyNoOtherCalls();
    }

    [Fact]
    public void PendingExitSaveDoesNotStartAnotherRequest()
    {
        // Repeated logout or quit attempts keep waiting on the original save request.
        Mock<IVoluntaryExitSaveRequest> request = new(MockBehavior.Strict);

        VoluntaryExitSaveAction action = VoluntaryExitSavePolicy.Start(
            playerEnteredWorld: true, savePending: true, request.Object);

        Assert.Equal(VoluntaryExitSaveAction.WaitForPendingSave, action);
        request.VerifyNoOtherCalls();
    }

    [Fact]
    public void SpawnedPlayerStartsExactlyOneExitSave()
    {
        // A spawned player delays exit after exactly one successful final-save request.
        Mock<IVoluntaryExitSaveRequest> request = new(MockBehavior.Strict);
        request.Setup(gateway => gateway.Request()).Returns(true);

        VoluntaryExitSaveAction action = VoluntaryExitSavePolicy.Start(
            playerEnteredWorld: true, savePending: false, request.Object);

        Assert.Equal(VoluntaryExitSaveAction.WaitForNewSave, action);
        request.Verify(gateway => gateway.Request(), Times.Once);
        request.VerifyNoOtherCalls();
    }

    [Fact]
    public void UnavailableSaveFallsBackWithoutRetry()
    {
        // If a final save cannot start, exit proceeds without an implicit retry loop.
        Mock<IVoluntaryExitSaveRequest> request = new(MockBehavior.Strict);
        request.Setup(gateway => gateway.Request()).Returns(false);

        VoluntaryExitSaveAction action = VoluntaryExitSavePolicy.Start(
            playerEnteredWorld: true, savePending: false, request.Object);

        Assert.Equal(VoluntaryExitSaveAction.PassThrough, action);
        request.Verify(gateway => gateway.Request(), Times.Once);
        request.VerifyNoOtherCalls();
    }
}
