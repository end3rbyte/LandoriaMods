using Xunit;

namespace Landoria.Socialize;

public sealed class GroupInvitationPolicyTests
{
    // Verifies that a non-leader cannot add an invitation to the group's pending invitations.
    [Fact]
    public void NonLeaderCannotInvitePlayer()
    {
        SocialGroup group = new() { Id = 1, Leader = 1 };
        group.Members[1] = "Leader";
        group.Members[2] = "Member";
        Dictionary<long, long> invitations = new();

        GroupDecision decision = GroupInvitationPolicy.TryInvite(
            group, inviter: 2, target: 3, targetAlreadyGrouped: false, invitations);

        Assert.False(decision.Allowed);
        Assert.Equal("Only the group leader can invite players.", decision.Message);
        Assert.Empty(invitations);
    }

    // Verifies that a leader records the invitation for the selected player.
    [Fact]
    public void LeaderRecordsInvitation()
    {
        SocialGroup group = new() { Id = 1, Leader = 1 };
        group.Members[1] = "Leader";
        Dictionary<long, long> invitations = new();

        GroupDecision decision = GroupInvitationPolicy.TryInvite(
            group, inviter: 1, target: 2, targetAlreadyGrouped: false, invitations);

        Assert.True(decision.Allowed);
        Assert.Equal(1, invitations[2]);
    }
}
