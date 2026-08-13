using Xunit;

namespace Landoria.SealedTombstone;

public sealed class TombstoneAccessPolicyTests
{
    // Verifies situations that retain vanilla tombstone interaction.
    [Theory]
    [InlineData(false, 2, 1, 5, 5)]
    [InlineData(true, 0, 1, 5, 5)]
    [InlineData(true, 1, 1, 5, 5)]
    [InlineData(true, 2, 1, 5, 15)]
    public void PublicOwnerAndExpiredTombstonesAllowInteraction(
        bool valid, long owner, long player, long lockDay, long currentDay)
    {
        TombstoneInteraction result = TombstoneAccessPolicy.Evaluate(
            valid, owner, player, lockDay, currentDay, blocked: false);

        Assert.Equal(TombstoneInteraction.Allow, result);
    }

    // Verifies that a recent foreign tombstone starts an owner access request.
    [Fact]
    public void RecentForeignTombstoneRequestsOwnerAccess()
    {
        TombstoneInteraction result = TombstoneAccessPolicy.Evaluate(
            true, ownerId: 2, playerId: 1, lockDay: 5, currentDay: 14, blocked: false);

        Assert.Equal(TombstoneInteraction.RequestAccess, result);
    }

    // Verifies that a recent attacker cannot request tombstone access.
    [Fact]
    public void BlockedAttackerCannotRequestAccess()
    {
        TombstoneInteraction result = TombstoneAccessPolicy.Evaluate(
            true, ownerId: 2, playerId: 1, lockDay: 5, currentDay: 5, blocked: true);

        Assert.Equal(TombstoneInteraction.Block, result);
    }

    // Verifies that the permanent attacker deny list still applies after public expiration.
    [Fact]
    public void BlockedAttackerRemainsBlockedAfterTenDays()
    {
        TombstoneInteraction result = TombstoneAccessPolicy.Evaluate(
            true, ownerId: 2, playerId: 1, lockDay: 5, currentDay: 15, blocked: true);

        Assert.Equal(TombstoneInteraction.Block, result);
    }

    // Verifies that legacy tombstones without a lock day remain protected.
    [Fact]
    public void MissingLockDayNeverExpiresTombstone()
    {
        Assert.False(TombstoneAccessPolicy.IsExpired(-1, 100));
        Assert.Equal(TombstoneInteraction.RequestAccess,
            TombstoneAccessPolicy.Evaluate(true, 2, 1, -1, 100, false));
    }

    // Verifies the exact ten-day public-access boundary.
    [Theory]
    [InlineData(5, 14, false)]
    [InlineData(5, 15, true)]
    [InlineData(5, 16, true)]
    [InlineData(-1, 15, false)]
    [InlineData(5, -1, false)]
    public void TombstoneExpirationUsesTenCompleteDays(
        long lockDay, long currentDay, bool expected)
    {
        Assert.Equal(expected, TombstoneAccessPolicy.IsExpired(lockDay, currentDay));
    }

    // Verifies that a request remains valid through exactly thirty seconds.
    [Theory]
    [InlineData(29, false)]
    [InlineData(30, false)]
    [InlineData(31, true)]
    public void RequestExpiresAfterThirtySeconds(int seconds, bool expected)
    {
        DateTime created = new(2026, 8, 13, 12, 0, 0, DateTimeKind.Utc);

        Assert.Equal(expected,
            TombstoneAccessPolicy.HasRequestExpired(created, created.AddSeconds(seconds)));
    }

    // Verifies that the two-minute cooldown ends at exactly 120 seconds.
    [Theory]
    [InlineData(119, true)]
    [InlineData(120, false)]
    [InlineData(121, false)]
    public void RequestCooldownUsesTwoMinutes(int seconds, bool expected)
    {
        DateTime requested = new(2026, 8, 13, 12, 0, 0, DateTimeKind.Utc);

        Assert.Equal(expected,
            TombstoneAccessPolicy.IsCooldownActive(requested, requested.AddSeconds(seconds)));
    }

    // Verifies player names are safe for popup and status-message markup.
    [Theory]
    [InlineData(null, "A player")]
    [InlineData("  ", "A player")]
    [InlineData("<b>Alice</b>", "bAlice/b")]
    public void RequesterNameIsSanitized(string name, string expected)
    {
        Assert.Equal(expected, TombstoneAccessPolicy.SafeName(name));
    }

    // Verifies player names are capped at 64 characters.
    [Fact]
    public void RequesterNameIsLimitedToSixtyFourCharacters()
    {
        Assert.Equal(64, TombstoneAccessPolicy.SafeName(new string('a', 80)).Length);
    }

    // Verifies that an offline owner reports immediately without starting the cooldown.
    [Fact]
    public void OfflineOwnerDoesNotStartRequestCooldown()
    {
        DateTime previous = new(2026, 8, 13, 11, 0, 0, DateTimeKind.Utc);

        TombstoneAvailabilityResult result = TombstoneRequestPolicy.ApplyAvailability(
            false, previous, previous.AddHours(1));

        Assert.Equal(previous, result.LastRequestAt);
        Assert.Equal("The tombstone owner is offline.", result.Message);
    }

    // Verifies that an online owner starts cooldown and confirms request delivery.
    [Fact]
    public void OnlineOwnerStartsRequestCooldown()
    {
        DateTime previous = new(2026, 8, 13, 11, 0, 0, DateTimeKind.Utc);
        DateTime now = previous.AddHours(1);

        TombstoneAvailabilityResult result = TombstoneRequestPolicy.ApplyAvailability(
            true, previous, now);

        Assert.Equal(now, result.LastRequestAt);
        Assert.Equal("Access request sent to the tombstone owner.", result.Message);
    }

    // Verifies the requester messages for approval, denial, and timeout.
    [Theory]
    [InlineData(true, "Alice granted access to the tombstone.")]
    [InlineData(false, "Alice denied or did not answer the request.")]
    public void DecisionMessageExplainsOutcome(bool accepted, string expected)
    {
        Assert.Equal(expected, TombstoneRequestPolicy.DecisionMessage(accepted, "Alice"));
    }

    // Verifies the vanilla Yes/No popup content shown to the owner.
    [Fact]
    public void OwnerPopupNamesRequesterAndTombstoneAction()
    {
        TombstoneRequestPresentation result = TombstonePresentationPolicy.Build("<Alice>");

        Assert.Equal("Tombstone access request", result.Title);
        Assert.Equal("Allow Alice to loot this tombstone?", result.Message);
    }
}
