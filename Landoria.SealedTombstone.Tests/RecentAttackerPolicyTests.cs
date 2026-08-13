using Xunit;

namespace Landoria.SealedTombstone;

public sealed class RecentAttackerPolicyTests
{
    // Verifies that only another player damaging the local player is recorded.
    [Theory]
    [InlineData(true, true, 1, 2, true)]
    [InlineData(false, true, 1, 2, false)]
    [InlineData(true, false, 1, 2, false)]
    [InlineData(true, true, 1, 0, false)]
    [InlineData(true, true, 1, 1, false)]
    public void OnlyRemotePlayerAttackersAreRecorded(
        bool localVictim, bool playerAttacker, long victim, long attacker, bool expected)
    {
        Assert.Equal(expected, RecentAttackerPolicy.ShouldRecord(
            localVictim, playerAttacker, victim, attacker));
    }

    // Verifies that only attackers from the two minutes before death are captured.
    [Fact]
    public void DeathSnapshotKeepsOnlyRecentAttackers()
    {
        DateTime death = new(2026, 8, 13, 12, 0, 0, DateTimeKind.Utc);
        Dictionary<long, DateTime> hits = new()
        {
            [1] = death.AddSeconds(-119),
            [2] = death.AddSeconds(-120),
            [3] = death.AddSeconds(-121)
        };

        long[] snapshot = RecentAttackerPolicy.Snapshot(hits, death);

        Assert.Contains(1, snapshot);
        Assert.Contains(2, snapshot);
        Assert.DoesNotContain(3, snapshot);
    }

    // Verifies attacker identifiers round-trip through tombstone storage format.
    [Fact]
    public void SerializedAttackersCanBeLookedUpExactly()
    {
        string serialized = RecentAttackerPolicy.Serialize(new long[] { 12, 123, 456 });

        Assert.Equal("12,123,456", serialized);
        Assert.True(RecentAttackerPolicy.Contains(serialized, 123));
        Assert.False(RecentAttackerPolicy.Contains(serialized, 23));
    }

    // Verifies empty attacker storage blocks nobody.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmptyAttackerListContainsNobody(string serialized)
    {
        Assert.False(RecentAttackerPolicy.Contains(serialized, 1));
    }
}
