using Moq;
using Xunit;

namespace Landoria.Parked;

public sealed class PieceAccessPolicyTests
{
    // Verifies access that does not require consulting group membership.
    [Theory]
    [InlineData(false, 0, 8, true)]
    [InlineData(true, 7, 0, true)]
    [InlineData(true, 7, 7, true)]
    [InlineData(true, 0, 7, false)]
    public void ImmediateAccessRulesBypassGroupLookup(
        bool placed, long player, long creator, bool expected)
    {
        Mock<Func<long, long, bool>> groups = new(MockBehavior.Strict);

        bool actual = PieceAccessPolicy.CanAccess(
            placed, player, creator, false, groups.Object);

        Assert.Equal(expected, actual);
        groups.VerifyNoOtherCalls();
    }

    // Verifies that missing client membership state denies access to another player's piece.
    [Fact]
    public void MissingMembershipStateDeniesForeignPiece()
    {
        Mock<Func<long, long, bool>> groups = new(MockBehavior.Strict);

        bool allowed = PieceAccessPolicy.CanAccess(true, 1, 2, false, groups.Object);

        Assert.False(allowed);
        groups.VerifyNoOtherCalls();
    }

    // Verifies that synchronized group membership decides foreign-piece access.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GroupMembershipDecidesForeignPieceAccess(bool sameGroup)
    {
        Mock<Func<long, long, bool>> groups = new(MockBehavior.Strict);
        groups.Setup(check => check(1, 2)).Returns(sameGroup);

        bool allowed = PieceAccessPolicy.CanAccess(true, 1, 2, true, groups.Object);

        Assert.Equal(sameGroup, allowed);
        groups.Verify(check => check(1, 2), Times.Once);
    }
}
