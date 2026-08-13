using Xunit;

namespace Landoria.SealedTombstone;

public sealed class TombstoneDecisionPolicyTests
{
    // Verifies that the server forwards a decision from the tombstone's authenticated owner.
    [Fact]
    public void AuthenticatedOwnerDecisionIsForwarded()
    {
        Assert.True(TombstoneDecisionPolicy.CanForward(
            true, true, mappedPlayerId: 10, expectedOwnerId: 10, tombstoneOwnerId: 10));
    }

    // Verifies every invalid decision source is rejected by the server.
    [Theory]
    [InlineData(false, true, 10, 10, 10)]
    [InlineData(true, false, 10, 10, 10)]
    [InlineData(true, true, 0, 10, 10)]
    [InlineData(true, true, 11, 10, 10)]
    [InlineData(true, true, 10, 10, 11)]
    public void UnauthenticatedOrStaleDecisionIsRejected(
        bool server, bool peer, long mapped, long expectedOwner, long tombstoneOwner)
    {
        Assert.False(TombstoneDecisionPolicy.CanForward(
            server, peer, mapped, expectedOwner, tombstoneOwner));
    }

    // Verifies that clients accept a decision only from their current server.
    [Theory]
    [InlineData(false, 20, true, 20, true)]
    [InlineData(false, 21, true, 20, false)]
    [InlineData(false, 0, false, 0, false)]
    [InlineData(true, 0, false, 0, true)]
    [InlineData(true, 20, false, 0, false)]
    public void DecisionResponseMustComeFromServer(
        bool serverHost, long sender, bool hasServerPeer, long serverPeer, bool expected)
    {
        Assert.Equal(expected,
            TombstoneDecisionPolicy.IsTrustedResponse(
                serverHost, sender, hasServerPeer, serverPeer));
    }

    // Verifies that only an accepted owner decision triggers permanent unlock handling.
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void OnlyApprovalUnlocksTombstone(bool accepted, bool expected)
    {
        Assert.Equal(expected, TombstoneDecisionPolicy.ShouldUnlock(accepted));
    }
}
