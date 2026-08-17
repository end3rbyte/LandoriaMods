namespace Landoria.ModSentry.Tests;

using Xunit;

public sealed class GuestAdmissionRegistryTests
{
    [Fact]
    public void CountdownStartsOnlyAfterPlayerIsReady()
    {
        float now = 5f;
        var registry = new GuestAdmissionRegistry<string>(() => now, 30);
        var notifications = new List<int>();
        registry.Add("peer");

        registry.Tick(_ => false, (_, seconds, _) => notifications.Add(seconds), _ => { });
        now = 20f;
        registry.Tick(_ => true, (_, seconds, _) => notifications.Add(seconds), _ => { });

        Assert.Equal(new[] { 30 }, notifications);
    }

    [Fact]
    public void DisconnectsAtZeroAndRemovesGuest()
    {
        float now = 0f;
        var registry = new GuestAdmissionRegistry<string>(() => now, 30);
        bool disconnected = false;
        registry.Add("peer");
        registry.Tick(_ => true, (_, _, _) => { }, _ => disconnected = true);

        now = 30f;
        registry.Tick(_ => true, (_, _, _) => { }, _ => disconnected = true);

        Assert.True(disconnected);
        Assert.False(registry.Contains("peer"));
    }
}
