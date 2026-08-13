using Xunit;

namespace Landoria.ModSentry;

public sealed class HandshakeRegistryTests
{
    // Verifies that accepting a peer removes its previous rejection.
    [Fact]
    public void AcceptReplacesRejection()
    {
        HandshakeRegistry<string> registry = new();
        registry.Reject("peer", Rejection("old"));

        registry.Accept("peer");

        Assert.True(registry.IsAccepted("peer"));
        Assert.Null(registry.RejectionFor("peer"));
    }

    // Verifies that rejecting a peer revokes its previous acceptance.
    [Fact]
    public void RejectReplacesAcceptance()
    {
        HandshakeRegistry<string> registry = new();
        ValidationResult rejection = Rejection("denied");
        registry.Accept("peer");

        registry.Reject("peer", rejection);

        Assert.False(registry.IsAccepted("peer"));
        Assert.Same(rejection, registry.RejectionFor("peer"));
    }

    // Verifies that removing one peer leaves other handshake states intact.
    [Fact]
    public void RemoveOnlyClearsSelectedPeer()
    {
        HandshakeRegistry<string> registry = new();
        registry.Accept("accepted");
        registry.Reject("rejected", Rejection("denied"));

        registry.Remove("accepted");

        Assert.False(registry.IsAccepted("accepted"));
        Assert.NotNull(registry.RejectionFor("rejected"));
    }

    // Verifies that clearing the registry removes accepted and rejected peers.
    [Fact]
    public void ClearRemovesEveryState()
    {
        HandshakeRegistry<string> registry = new();
        registry.Accept("accepted");
        registry.Reject("rejected", Rejection("denied"));

        registry.Clear();

        Assert.False(registry.IsAccepted("accepted"));
        Assert.Null(registry.RejectionFor("rejected"));
    }

    private static ValidationResult Rejection(string message)
    {
        return ValidationResult.Reject(message, message);
    }
}
