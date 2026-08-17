using Xunit;

namespace Landoria.ModSentry.Tests;

public sealed class GuestPrisonPositionTests
{
    [Theory]
    [InlineData("100,500,200", 100f, 500f, 200f)]
    [InlineData(" -1.5, 42.25, 3 ", -1.5f, 42.25f, 3f)]
    public void ParsesInvariantCoordinates(string value, float x, float y, float z)
    {
        Assert.True(GuestPrisonPosition.TryParse(value,
            out float actualX, out float actualY, out float actualZ));
        Assert.Equal((x, y, z), (actualX, actualY, actualZ));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1,2")]
    [InlineData("1,2,NaN")]
    [InlineData("1;2;3")]
    public void RejectsInvalidCoordinates(string value)
    {
        Assert.False(GuestPrisonPosition.TryParse(value, out _, out _, out _));
    }
}
