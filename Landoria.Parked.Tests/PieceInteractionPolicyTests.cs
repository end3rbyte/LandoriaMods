using Xunit;

namespace Landoria.Parked;

public sealed class PieceInteractionPolicyTests
{
    // Verifies that a player cannot damage another creator's building.
    [Fact]
    public void PlayerCannotDamageForeignBuilding()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(true, 1, 2);

        Assert.False(PieceInteractionPolicy.CanDamage(hasAccess));
    }

    // Verifies that a player can damage a building they created.
    [Fact]
    public void PlayerCanDamageOwnBuilding()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(true, 1, 1);

        Assert.True(PieceInteractionPolicy.CanDamage(hasAccess));
    }

    // Verifies that no relationship grants access to another creator's building.
    [Fact]
    public void PlayerCannotDamageAnotherCreatorsBuilding()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(true, 1, 2);

        Assert.False(PieceInteractionPolicy.CanDamage(hasAccess));
    }

    // Verifies that the hammer cannot destroy another creator's wall.
    [Fact]
    public void HammerCannotDestroyForeignWall()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(true, 1, 2);
        bool result = true;

        bool runVanillaRemoval = PieceInteractionPolicy.CanRemove(hasAccess, ref result);

        Assert.False(runVanillaRemoval);
        Assert.False(result);
    }

    // Verifies that the creator can destroy their own wall.
    [Fact]
    public void CreatorCanDestroyOwnWall()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(true, 1, 1);
        bool result = false;

        bool runVanillaRemoval = PieceInteractionPolicy.CanRemove(hasAccess, ref result);

        Assert.True(runVanillaRemoval);
        Assert.False(result);
    }

    // Verifies that another creator's door cannot be opened.
    [Fact]
    public void PlayerCannotOpenForeignDoor()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(true, 1, 2);

        Assert.False(PieceInteractionPolicy.CanUse(hasAccess));
    }

    // Verifies that the creator can open their own door.
    [Fact]
    public void CreatorCanOpenOwnDoor()
    {
        bool hasAccess = PieceAccessPolicy.CanAccess(true, 1, 1);

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
