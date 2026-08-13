using Xunit;

namespace Landoria.Socialize;

public sealed class PieceInteractionPolicyTests
{
    // Verifies that the hammer cannot destroy a wall created by a player outside the group.
    [Fact]
    public void HammerCannotDestroyForeignWall()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(
            placedByPlayer: true, playerId: 1, creator: 2,
            hasMembershipState: true, areGroupMembers: (_, _) => false);
        bool result = true;

        bool runVanillaRemoval = PieceInteractionPolicy.CanRemove(hasAccess, ref result);

        Assert.False(runVanillaRemoval);
        Assert.False(result);
    }

    // Verifies that the hammer can destroy a wall created by another group member.
    [Fact]
    public void HammerCanDestroyGroupMembersWall()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(
            placedByPlayer: true, playerId: 1, creator: 2,
            hasMembershipState: true, areGroupMembers: (_, _) => true);
        bool result = false;

        bool runVanillaRemoval = PieceInteractionPolicy.CanRemove(hasAccess, ref result);

        Assert.True(runVanillaRemoval);
        Assert.False(result);
    }

    // Verifies that a door created by a player outside the group cannot be opened.
    [Fact]
    public void PlayerCannotOpenForeignDoor()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(
            placedByPlayer: true, playerId: 1, creator: 2,
            hasMembershipState: true, areGroupMembers: (_, _) => false);

        Assert.False(PieceInteractionPolicy.CanUse(hasAccess));
    }

    // Verifies that a door created by another group member can be opened.
    [Fact]
    public void PlayerCanOpenGroupMembersDoor()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(
            placedByPlayer: true, playerId: 1, creator: 2,
            hasMembershipState: true, areGroupMembers: (_, _) => true);

        Assert.True(PieceInteractionPolicy.CanUse(hasAccess));
    }
}
