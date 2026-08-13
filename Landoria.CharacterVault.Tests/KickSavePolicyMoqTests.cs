using Moq;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class KickSavePolicyMoqTests
{
    [Fact]
    public void RejectedEligibilityProviderIsReadOnceAndSkipsTheSave()
    {
        // Verifies that a rejected session is resolved once and kicked without requesting a save.
        Mock<Func<int>> eligibility = new();
        eligibility.Setup(provider => provider()).Returns((int)KickSaveEligibility.Rejected);

        KickAction action = KickSavePolicy.Decide(validServerPeer: true,
            saveAuthorized: false, savePending: false,
            (KickSaveEligibility)eligibility.Object());

        Assert.Equal(KickAction.AllowWithoutSave, action);
        eligibility.Verify(provider => provider(), Times.Once);
        eligibility.VerifyNoOtherCalls();
    }
}
