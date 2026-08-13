using Moq;
using Xunit;

namespace Landoria.ModSentry;

public sealed class PendingDisconnectRegistryTests
{
    // Verifies that an acknowledgement cancels the fallback disconnect.
    [Fact]
    public void AcknowledgementCancelsDisconnect()
    {
        float time = 10;
        PendingDisconnectRegistry<string> registry = new(() => time, 2);
        Mock<Action<string>> disconnect = new(MockBehavior.Strict);
        registry.Schedule("peer");

        bool acknowledged = registry.Acknowledge("peer");
        time = 20;
        registry.Tick(disconnect.Object);

        Assert.True(acknowledged);
        disconnect.VerifyNoOtherCalls();
    }

    // Verifies that a pending peer disconnects exactly at the fallback deadline.
    [Fact]
    public void DeadlineDisconnectsPeerOnce()
    {
        float time = 10;
        PendingDisconnectRegistry<string> registry = new(() => time, 2);
        Mock<Action<string>> disconnect = new(MockBehavior.Strict);
        disconnect.Setup(action => action("peer"));
        registry.Schedule("peer");

        time = 11.99f;
        registry.Tick(disconnect.Object);
        time = 12;
        registry.Tick(disconnect.Object);
        registry.Tick(disconnect.Object);

        disconnect.Verify(action => action("peer"), Times.Once);
    }

    // Verifies that scheduling the same peer again replaces its deadline.
    [Fact]
    public void RescheduleExtendsDeadline()
    {
        float time = 10;
        PendingDisconnectRegistry<string> registry = new(() => time, 2);
        Mock<Action<string>> disconnect = new(MockBehavior.Strict);
        registry.Schedule("peer");
        time = 11;
        registry.Schedule("peer");

        time = 12;
        registry.Tick(disconnect.Object);

        disconnect.VerifyNoOtherCalls();
    }

    // Verifies that removing a peer cancels its pending fallback.
    [Fact]
    public void RemoveCancelsDisconnect()
    {
        float time = 0;
        PendingDisconnectRegistry<string> registry = new(() => time, 2);
        Mock<Action<string>> disconnect = new(MockBehavior.Strict);
        registry.Schedule("peer");

        registry.Remove("peer");
        time = 3;
        registry.Tick(disconnect.Object);

        disconnect.VerifyNoOtherCalls();
    }

    // Verifies that clear cancels every pending disconnect.
    [Fact]
    public void ClearCancelsAllDisconnects()
    {
        float time = 0;
        PendingDisconnectRegistry<string> registry = new(() => time, 2);
        Mock<Action<string>> disconnect = new(MockBehavior.Strict);
        registry.Schedule("one");
        registry.Schedule("two");

        registry.Clear();
        time = 3;
        registry.Tick(disconnect.Object);

        disconnect.VerifyNoOtherCalls();
    }
}
