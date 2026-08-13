using Moq;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class KickSaveRequestTests
{
    // Verifies that kicking a save-eligible player requests one final character save.
    [Fact]
    public void EligiblePlayerKickRequestsFinalSave()
    {
        Mock<IKickSaveRequest> saveRequest = new(MockBehavior.Strict);
        saveRequest
            .Setup(request => request.Request())
            .Returns(new KickSaveRequestResult(true, "server-disconnect-test"));
        KickAction action = KickSavePolicy.Decide(
            validServerPeer: true,
            saveAuthorized: false,
            savePending: false,
            eligibility: KickSaveEligibility.SaveRequired);

        KickSaveRequestResult result = KickSaveRequestExecutor.Execute(action, saveRequest.Object);

        Assert.Equal(KickAction.RequestSave, action);
        Assert.True(result.Started);
        Assert.Equal("server-disconnect-test", result.RequestId);
        saveRequest.Verify(request => request.Request(), Times.Once);
    }
}
