using Xunit;

namespace Landoria.CharacterVault;

public sealed class DocumentedSaveEventPolicyTests
{
    [Theory]
    [InlineData(0, 1, 2, true, false, 0)]
    [InlineData(1, 2, 1, true, true, 0)]
    [InlineData(2, 2, 1, true, true, 0)]
    [InlineData(3, 1, 1, true, true, 0)]
    [InlineData(4, 1, 1, true, false, 10)]
    [InlineData(5, 1, 1, true, false, 10)]
    [InlineData(6, 1, 2, true, false, 0)]
    [InlineData(7, 2, 2, true, false, 90)]
    [InlineData(8, 0, 0, false, false, 0)]
    public void EveryThunderstoreTableRowHasTheDocumentedBehavior(int eventValue,
        int target, int confirmation, bool requiresSpawn, bool preservesVanilla, int timeout)
    {
        DocumentedSaveBehavior behavior = DocumentedSaveEventPolicy.Get(
            (DocumentedSaveEvent)eventValue);

        Assert.Equal((SaveTarget)target, behavior.Target);
        Assert.Equal((SaveConfirmation)confirmation, behavior.Confirmation);
        Assert.Equal(requiresSpawn, behavior.RequiresSpawn);
        Assert.Equal(preservesVanilla, behavior.PreservesVanilla);
        Assert.Equal(timeout, behavior.TimeoutSeconds);
    }

    [Fact]
    public void EveryDeclaredEventIsCoveredByThePolicy()
    {
        foreach (DocumentedSaveEvent saveEvent in Enum.GetValues<DocumentedSaveEvent>())
        {
            Assert.NotNull(DocumentedSaveEventPolicy.Get(saveEvent));
        }
    }

    [Fact]
    public void UnknownEventIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DocumentedSaveEventPolicy.Get((DocumentedSaveEvent)int.MaxValue));
    }
}
