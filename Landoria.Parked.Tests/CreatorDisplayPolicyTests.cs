using Xunit;

namespace Landoria.Parked;

public sealed class CreatorDisplayPolicyTests
{
    // Verifies that the creator name stored on a new piece has priority.
    [Fact]
    public void StoredCreatorNameHasPriority()
    {
        Assert.Equal("Stored", CreatorDisplayPolicy.ResolveName("Stored", "Known"));
    }

    // Verifies that legacy pieces use the creator name known by Parked.
    [Fact]
    public void LegacyPieceUsesKnownCreatorName()
    {
        Assert.Equal("Known", CreatorDisplayPolicy.ResolveName("", "Known"));
    }

    // Verifies that an unidentified legacy creator receives the documented fallback.
    [Fact]
    public void UnknownLegacyCreatorUsesFallback()
    {
        Assert.Equal("Unknown creator", CreatorDisplayPolicy.ResolveName(" ", null));
    }

    // Verifies the creator line appended below existing hover text.
    [Fact]
    public void CreatorIsAppendedToExistingHoverText()
    {
        Assert.Equal("Door\n<color=orange>Created by Alice</color>",
            CreatorDisplayPolicy.AppendCreator("Door", "Alice"));
    }
}
