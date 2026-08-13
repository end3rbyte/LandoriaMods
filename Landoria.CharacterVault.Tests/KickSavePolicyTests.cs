using Xunit;

namespace Landoria.CharacterVault;

public sealed class KickSavePolicyTests
{
    // Verifies all kick outcomes across peer validity, authorization, pending saves, and eligibility.
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

    // Verifies that a rejected player is kicked without creating a character save.
    [Fact]
    public void RejectedPlayerNeverWaitsForCharacterSave()
    {
        KickAction action = KickSavePolicy.Decide(true, false, false,
            KickSaveEligibility.Rejected);

        Assert.Equal(KickAction.AllowWithoutSave, action);
    }

    // Verifies that a permitted player must save successfully before the server kick proceeds.
    [Fact]
    public void PermittedPlayerRequiresSaveBeforeKick()
    {
        KickAction action = KickSavePolicy.Decide(true, false, false,
            KickSaveEligibility.SaveRequired);

        Assert.Equal(KickAction.RequestSave, action);
    }
}
