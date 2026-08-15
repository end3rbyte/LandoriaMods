using Xunit;

namespace Landoria.Parked;

public sealed class PieceAccessPolicyTests
{
    [Theory]
    [InlineData(false, 0, 8, true)]
    [InlineData(true, 7, 0, true)]
    [InlineData(true, 7, 7, true)]
    [InlineData(true, 0, 7, false)]
    [InlineData(true, 7, 8, false)]
    public void OnlyCreatorCanAccessPlayerBuiltPiece(
        bool placed, long player, long creator, bool expected)
    {
        Assert.Equal(expected, PieceAccessPolicy.CanAccess(placed, player, creator));
    }
}
