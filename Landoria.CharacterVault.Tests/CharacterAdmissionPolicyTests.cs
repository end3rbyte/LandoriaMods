using Xunit;

namespace Landoria.CharacterVault;

public sealed class CharacterAdmissionPolicyTests
{
    [Theory]
    [InlineData(true, false, false, true, false, 0)]
    [InlineData(true, true, true, true, true, 0)]
    [InlineData(false, false, true, false, true, 2)]
    [InlineData(false, true, false, true, true, 3)]
    [InlineData(false, true, true, true, true, 1)]
    [InlineData(false, true, false, false, true, 1)]
    [InlineData(false, true, true, false, false, 4)]
    public void DecideCoversAdmissionMatrix(bool stored, bool created, bool multiple,
        bool accountHasProfile, bool reservation, int expected)
    {
        CharacterAdmission actual = CharacterAdmissionPolicy.Decide(stored, created, multiple,
            accountHasProfile, reservation);

        Assert.Equal((CharacterAdmission)expected, actual);
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(true, true, true, true)]
    public void ServerSaveRequiresVerifiedAdmittedAndPermitted(bool verified, bool admitted,
        bool permitted, bool expected)
    {
        ServerProfileSessionState state = new()
        {
            Verified = verified,
            Admitted = admitted
        };
        state.RecordPermission(permitted);

        Assert.Equal(expected, state.CanSave);
    }

    [Fact]
    public void PermissionRevocationImmediatelyBlocksServerSaves()
    {
        ServerProfileSessionState state = new() { Verified = true, Admitted = true };
        state.RecordPermission(true);
        Assert.True(state.CanSave);

        state.RecordPermission(false);

        Assert.False(state.CanSave);
    }
}
