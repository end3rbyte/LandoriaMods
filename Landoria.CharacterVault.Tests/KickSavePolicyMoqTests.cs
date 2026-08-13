using Moq;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class KickSavePolicyMoqTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void NonRequestActionsNeverInvokeTheSaveGateway(int action)
    {
        // Non-save kick outcomes must not contact the character save subsystem.
        Mock<IKickSaveRequest> request = new(MockBehavior.Strict);

        KickSaveRequestResult result = KickSaveRequestExecutor.Execute(
            (KickAction)action, request.Object);

        Assert.False(result.Started);
        Assert.Empty(result.RequestId);
        request.VerifyNoOtherCalls();
    }

    [Fact]
    public void RequestSaveInvokesTheGatewayOnceAndReturnsItsRequestId()
    {
        // A permitted kick delegates exactly one final-save request and preserves its identifier.
        Mock<IKickSaveRequest> request = new(MockBehavior.Strict);
        request.Setup(gateway => gateway.Request())
            .Returns(new KickSaveRequestResult(true, "request-1"));

        KickSaveRequestResult result = KickSaveRequestExecutor.Execute(
            KickAction.RequestSave, request.Object);

        Assert.True(result.Started);
        Assert.Equal("request-1", result.RequestId);
        request.Verify(gateway => gateway.Request(), Times.Once);
        request.VerifyNoOtherCalls();
    }

    [Fact]
    public void FailedSaveRequestIsReturnedWithoutRetry()
    {
        // A failed save start remains blocked and is not retried implicitly.
        Mock<IKickSaveRequest> request = new(MockBehavior.Strict);
        request.Setup(gateway => gateway.Request())
            .Returns(new KickSaveRequestResult(false));

        KickSaveRequestResult result = KickSaveRequestExecutor.Execute(
            KickAction.RequestSave, request.Object);

        Assert.False(result.Started);
        request.Verify(gateway => gateway.Request(), Times.Once);
        request.VerifyNoOtherCalls();
    }
}
