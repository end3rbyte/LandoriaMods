using Moq;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class StartingItemGrantTests
{
    // Verifies that a Hammer is added to the new profile's inventory once it spawns.
    [Fact]
    public void NewProfileReceivesConfiguredHammer()
    {
        object hammer = new();
        Mock<Func<string, object>> findItem = new(MockBehavior.Strict);
        Mock<Func<object, int, bool>> addItem = new(MockBehavior.Strict);
        findItem.Setup(find => find("Hammer")).Returns(hammer);
        addItem.Setup(add => add(hammer, 1)).Returns(true);

        bool granted = StartingItemGrantPolicy.Grant(
            "Hammer", 1, findItem.Object, addItem.Object);

        Assert.True(granted);
        findItem.Verify(find => find("Hammer"), Times.Once);
        addItem.Verify(add => add(hammer, 1), Times.Once);
    }
}
