using Xunit;

namespace Landoria.Socialize;

public sealed class GroupAcceptancePolicyTests
{
    // Verifies that accepting a valid invitation adds the player to both membership indexes.
    [Fact]
    public void ValidInvitationAddsPlayerAndConsumesInvitation()
    {
        SocialGroup group = Group(1, 1);
        Dictionary<long, long> invitations = new() { [2] = 1 };
        Dictionary<long, int> playerGroups = new() { [1] = group.Id };

        GroupAcceptanceResult result = GroupAcceptancePolicy.Accept(
            2, "Alice", "1", invitations, _ => group, playerGroups);

        Assert.True(result.Accepted);
        Assert.Same(group, result.Group);
        Assert.Equal("Alice", group.Members[2]);
        Assert.Equal(group.Id, playerGroups[2]);
        Assert.DoesNotContain(2, invitations.Keys);
    }

    // Verifies malformed, missing, and mismatched invitations are rejected without mutation.
    [Theory]
    [InlineData("invalid", false)]
    [InlineData("1", false)]
    [InlineData("1", true)]
    public void InvalidInvitationDoesNotAddPlayer(string inviterText, bool mismatched)
    {
        SocialGroup group = Group(1, 1);
        Dictionary<long, long> invitations = new();
        if (mismatched)
        {
            invitations[2] = 9;
        }
        Dictionary<long, int> playerGroups = new() { [1] = group.Id };

        GroupAcceptanceResult result = GroupAcceptancePolicy.Accept(
            2, "Alice", inviterText, invitations, _ => group, playerGroups);

        Assert.False(result.Accepted);
        Assert.Equal("That group invitation is no longer valid.", result.Message);
        Assert.DoesNotContain(2, group.Members.Keys);
        Assert.DoesNotContain(2, playerGroups.Keys);
    }

    // Verifies an invitation cannot be accepted after the group reaches maximum size.
    [Fact]
    public void GroupFilledAfterInvitationRejectsAcceptance()
    {
        SocialGroup group = Group(1, SocialGroup.MaximumSize);
        Dictionary<long, long> invitations = new() { [6] = 1 };
        Dictionary<long, int> playerGroups = new();

        GroupAcceptanceResult result = GroupAcceptancePolicy.Accept(
            6, "Late", "1", invitations, _ => group, playerGroups);

        Assert.False(result.Accepted);
        Assert.Equal("That group is no longer available.", result.Message);
        Assert.DoesNotContain(6, group.Members.Keys);
        Assert.DoesNotContain(6, playerGroups.Keys);
        Assert.Equal(1, invitations[6]);
    }

    // Verifies acceptance fails when the inviter no longer owns an available group.
    [Fact]
    public void MissingInviterGroupRejectsAcceptance()
    {
        Dictionary<long, long> invitations = new() { [2] = 1 };

        GroupAcceptanceResult result = GroupAcceptancePolicy.Accept(
            2, "Alice", "1", invitations, _ => null, new Dictionary<long, int>());

        Assert.False(result.Accepted);
        Assert.Equal("That group is no longer available.", result.Message);
    }

    private static SocialGroup Group(long leader, int memberCount)
    {
        SocialGroup group = new() { Id = 7, Leader = leader };
        for (int player = 1; player <= memberCount; player++)
        {
            group.Members[player] = "Player" + player;
        }
        return group;
    }
}
