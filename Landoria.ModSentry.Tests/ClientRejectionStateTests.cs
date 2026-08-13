using Xunit;

namespace Landoria.ModSentry;

public sealed class ClientRejectionStateTests
{
    // Verifies that the server rejection message remains available for the menu.
    [Fact]
    public void ReceiveStoresMessage()
    {
        ClientRejectionState state = new();

        state.Receive("Required mod missing.", 12);

        Assert.True(state.TryGet(out string message));
        Assert.Equal("Required mod missing.", message);
    }

    // Verifies that an empty message is not displayed.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyMessageIsNotAvailable(string message)
    {
        ClientRejectionState state = new();
        state.Receive(message!, 12);

        Assert.False(state.TryGet(out _));
    }

    // Verifies that the client waits while still connecting before the fallback deadline.
    [Fact]
    public void ConnectingClientWaitsBeforeDeadline()
    {
        ClientRejectionState state = new();
        state.Receive("Rejected", 12);

        Assert.False(state.TryBeginReturnToMenu(isConnecting: true, currentTime: 11.99f));
    }

    // Verifies that server disconnection allows an immediate return to the menu.
    [Fact]
    public void DisconnectedClientReturnsBeforeDeadline()
    {
        ClientRejectionState state = new();
        state.Receive("Rejected", 12);

        Assert.True(state.TryBeginReturnToMenu(isConnecting: false, currentTime: 10));
    }

    // Verifies that the timeout returns a still-connecting client to the menu once.
    [Fact]
    public void DeadlineReturnsConnectingClientOnce()
    {
        ClientRejectionState state = new();
        state.Receive("Rejected", 12);

        Assert.True(state.TryBeginReturnToMenu(isConnecting: true, currentTime: 12));
        Assert.False(state.TryBeginReturnToMenu(isConnecting: true, currentTime: 13));
    }

    // Verifies that clearing removes the message and pending menu transition.
    [Fact]
    public void ClearResetsMessageAndReturnState()
    {
        ClientRejectionState state = new();
        state.Receive("Rejected", 12);

        state.Clear();

        Assert.False(state.TryGet(out _));
        Assert.False(state.TryBeginReturnToMenu(isConnecting: false, currentTime: 20));
    }
}
