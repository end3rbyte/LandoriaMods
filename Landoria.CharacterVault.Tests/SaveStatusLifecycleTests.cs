using Xunit;

namespace Landoria.CharacterVault;

public sealed class SaveStatusLifecycleTests
{
    [Fact]
    public void CommitRequiresTheCurrentAcceptedRequest()
    {
        SaveStatusLifecycle lifecycle = new();
        lifecycle.Begin("request-a", waitingForCommit: true);

        Assert.True(lifecycle.CanCommit("request-a"));
        Assert.False(lifecycle.CanCommit("request-b"));
    }

    [Fact]
    public void SavingStateCannotReceiveCommitConfirmation()
    {
        SaveStatusLifecycle lifecycle = new();
        lifecycle.Begin("request-a", waitingForCommit: false);

        Assert.False(lifecycle.CanCommit("request-a"));
    }

    [Fact]
    public void TimeoutRequiresMatchingRequestAndVersion()
    {
        SaveStatusLifecycle lifecycle = new();
        int version = lifecycle.Begin("request-a", waitingForCommit: true);

        Assert.True(lifecycle.CanFail("request-a", version));
        Assert.False(lifecycle.CanFail("request-b", version));
        Assert.False(lifecycle.CanFail("request-a", version + 1));
    }

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
