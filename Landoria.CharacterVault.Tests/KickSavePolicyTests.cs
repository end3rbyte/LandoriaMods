using Xunit;

namespace Landoria.CharacterVault;

public sealed class KickSavePolicyTests
{
    [Theory]
    [InlineData(false, false, false, 0, 0)]
    [InlineData(true, true, false, 2, 0)]
    [InlineData(true, false, false, 1, 1)]
    [InlineData(true, false, true, 2, 2)]
    [InlineData(true, false, false, 2, 3)]
    [InlineData(true, false, false, 0, 4)]
    public void DecideCoversKickMatrix(bool validPeer, bool authorized, bool pending,
        int eligibility, int expected)
    {
        KickAction actual = KickSavePolicy.Decide(validPeer, authorized, pending,
            (KickSaveEligibility)eligibility);

        Assert.Equal((KickAction)expected, actual);
    }

    [Fact]
    public void RejectedPlayerNeverWaitsForCharacterSave()
    {
        KickAction action = KickSavePolicy.Decide(true, false, false,
            KickSaveEligibility.Rejected);

        Assert.Equal(KickAction.AllowWithoutSave, action);
    }

    [Fact]
    public void PermittedPlayerRequiresSaveBeforeKick()
    {
        KickAction action = KickSavePolicy.Decide(true, false, false,
            KickSaveEligibility.SaveRequired);

        Assert.Equal(KickAction.RequestSave, action);
    }
}
