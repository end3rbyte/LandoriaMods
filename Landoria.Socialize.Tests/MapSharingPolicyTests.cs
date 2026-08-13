using System.Collections.Generic;
using Xunit;

namespace Landoria.Socialize;

public sealed class MapSharingPolicyTests
{
    // Verifies that Socialize always disables vanilla public position sharing.
    [Fact]
    public void PublicPositionIsAlwaysDisabled()
    {
        Assert.False(MapSharingPolicy.GetPublicPosition());
    }

    // Verifies that public map pings require group membership.
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void PublicPingVisibilityFollowsGroupMembership(bool inGroup, bool expected)
    {
        Assert.Equal(expected, MapSharingPolicy.CanSendPublicPing(inGroup));
    }

    // Verifies that a connected group member missing from the public list is added.
    [Fact]
    public void MissingRemoteGroupMemberIsAddedToMap()
    {
        Assert.True(MapSharingPolicy.ShouldAddGroupMember(2, 1, new HashSet<long> { 3 }));
    }

    // Verifies that the local player and already visible members are not duplicated.
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    public void LocalOrVisibleGroupMemberIsNotAdded(long member, long local)
    {
        Assert.False(MapSharingPolicy.ShouldAddGroupMember(
            member, local, new HashSet<long> { 2 }));
    }

    // Verifies that a map ping is delivered only to the sender and connected group members.
    [Fact]
    public void GroupPingExcludesOutsidersAndOfflineMembers()
    {
        HashSet<long> group = new() { 1, 2, 3 };

        List<long> recipients = MapSharingPolicy.GetGroupPingRecipients(
            1, group, new long[] { 1, 2, 4 });

        Assert.Equal([1L, 2L], recipients);
        Assert.DoesNotContain(3, recipients);
        Assert.DoesNotContain(4, recipients);
    }

    // Verifies that a player outside a group cannot produce any group-ping recipient.
    [Fact]
    public void PlayerOutsideGroupCannotSendGroupPing()
    {
        List<long> recipients = MapSharingPolicy.GetGroupPingRecipients(
            1, new HashSet<long> { 2, 3 }, new long[] { 1, 2, 3 });

        Assert.Empty(recipients);
    }

    // Verifies that repeated connected-player entries never duplicate ping delivery.
    [Fact]
    public void GroupPingRecipientsAreUnique()
    {
        List<long> recipients = MapSharingPolicy.GetGroupPingRecipients(
            1, new HashSet<long> { 1, 2 }, new long[] { 2, 2, 1 });

        Assert.Equal([1L, 2L], recipients);
    }
}
