using Xunit;

namespace Landoria.CharacterVault;

public sealed class SaveStatusLifecycleTests
{
    // Verifies that commit confirmation is accepted only for the current acknowledged request.
    [Fact]
    public void CommitRequiresTheCurrentAcceptedRequest()
    {
        SaveStatusLifecycle lifecycle = new();
        lifecycle.Begin("request-a", waitingForCommit: true);

        Assert.True(lifecycle.CanCommit("request-a"));
        Assert.False(lifecycle.CanCommit("request-b"));
    }

    // Verifies that an upload still in its initial saving state cannot complete prematurely.
    [Fact]
    public void SavingStateCannotReceiveCommitConfirmation()
    {
        SaveStatusLifecycle lifecycle = new();
        lifecycle.Begin("request-a", waitingForCommit: false);

        Assert.False(lifecycle.CanCommit("request-a"));
    }

    // Verifies that failure timeout callbacks match both request identity and state version.
    [Fact]
    public void TimeoutRequiresMatchingRequestAndVersion()
    {
        SaveStatusLifecycle lifecycle = new();
        int version = lifecycle.Begin("request-a", waitingForCommit: true);

        Assert.True(lifecycle.CanFail("request-a", version));
        Assert.False(lifecycle.CanFail("request-b", version));
        Assert.False(lifecycle.CanFail("request-a", version + 1));
    }

    // Verifies that a newer request invalidates timeout and hide callbacks from the prior one.
    [Fact]
    public void NewRequestInvalidatesPreviousTimeoutAndHideTimers()
    {
        SaveStatusLifecycle lifecycle = new();
        int previous = lifecycle.Begin("request-a", waitingForCommit: true);

        int current = lifecycle.Begin("request-b", waitingForCommit: false);

        Assert.False(lifecycle.CanFail("request-a", previous));
        Assert.False(lifecycle.IsCurrent(previous));
        Assert.True(lifecycle.IsCurrent(current));
    }

    // Verifies that clearing the display invalidates every outstanding status callback.
    [Fact]
    public void ClearInvalidatesAllPendingCallbacks()
    {
        SaveStatusLifecycle lifecycle = new();
        int version = lifecycle.Begin("request-a", waitingForCommit: true);

        lifecycle.Clear();

        Assert.False(lifecycle.CanCommit("request-a"));
        Assert.False(lifecycle.CanFail("request-a", version));
        Assert.False(lifecycle.IsCurrent(version));
    }
}
