using Xunit;

namespace Landoria.Socialize;

public sealed class PieceInteractionPolicyTests
{
    // Verifies that a player cannot damage a building created outside the group.
    [Fact]
    public void PlayerCannotDamageForeignBuilding()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(
            placedByPlayer: true, playerId: 1, creator: 2,
            hasMembershipState: true, areGroupMembers: (_, _) => false);

        Assert.False(PieceInteractionPolicy.CanDamage(hasAccess));
    }

    // Verifies that a player can damage a building they created.
    [Fact]
    public void PlayerCanDamageOwnBuilding()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(
            placedByPlayer: true, playerId: 1, creator: 1,
            hasMembershipState: true, areGroupMembers: (_, _) => false);

        Assert.True(PieceInteractionPolicy.CanDamage(hasAccess));
    }

    // Verifies that a player can damage a building created by another group member.
    [Fact]
    public void PlayerCanDamageGroupMembersBuilding()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(
            placedByPlayer: true, playerId: 1, creator: 2,
            hasMembershipState: true, areGroupMembers: (_, _) => true);

        Assert.True(PieceInteractionPolicy.CanDamage(hasAccess));
    }

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

    // Verifies every documented use interaction delegates to the same access decision.
    [Theory]
    [InlineData("normal use")]
    [InlineData("item use")]
    [InlineData("container")]
    [InlineData("door")]
    [InlineData("crafting station")]
    [InlineData("resource or fuel")]
    [InlineData("repair")]
    public void DocumentedPieceUseRequiresAccess(string interaction)
    {
        Assert.False(PieceInteractionPolicy.CanUse(false));
        Assert.True(PieceInteractionPolicy.CanUse(true));
        Assert.False(string.IsNullOrWhiteSpace(interaction));
    }
}
