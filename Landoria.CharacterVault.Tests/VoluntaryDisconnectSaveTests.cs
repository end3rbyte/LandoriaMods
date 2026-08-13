using Moq;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class VoluntaryDisconnectSaveTests
{
    // Verifies that a voluntary disconnect requests one final character save.
    [Fact]
    public void VoluntaryDisconnectRequestsFinalSave()
    {
        Mock<IVoluntaryExitSaveRequest> saveRequest = new(MockBehavior.Strict);
        saveRequest
            .Setup(request => request.Request())
            .Returns(true);

        VoluntaryExitSaveAction action = VoluntaryExitSavePolicy.Start(
            playerEnteredWorld: true,
            savePending: false,
            saveRequest.Object);

        Assert.Equal(VoluntaryExitSaveAction.WaitForNewSave, action);
        saveRequest.Verify(request => request.Request(), Times.Once);
    }
}
