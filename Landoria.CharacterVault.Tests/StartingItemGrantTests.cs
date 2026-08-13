using Moq;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class StartingItemGrantTests
{
    // Verifies that first spawn grants the Hammer once and saves the enrolled profile once.
    [Fact]
    public void NewProfileReceivesConfiguredHammerAndIsSavedOnce()
    {
        ClientSaveLifecycle lifecycle = new();
        lifecycle.BeginEnrollment();
        object hammer = new();
        Mock<Func<string, object>> findItem = new(MockBehavior.Strict);
        Mock<Func<object, int, bool>> addItem = new(MockBehavior.Strict);
        Mock<Action> saveProfile = new(MockBehavior.Strict);
        Mock<Action<StartingItem>> reportFailure = new(MockBehavior.Strict);
        findItem.Setup(find => find("Hammer")).Returns(hammer);
        addItem.Setup(add => add(hammer, 1)).Returns(true);
        saveProfile.Setup(save => save());

        bool firstSpawnHandled = StartingItemGrantPolicy.ApplyEnrollment(lifecycle, true,
            new[] { new StartingItem("Hammer", 1) }, findItem.Object, addItem.Object,
            saveProfile.Object, reportFailure.Object);
        bool secondSpawnHandled = StartingItemGrantPolicy.ApplyEnrollment(lifecycle, true,
            new[] { new StartingItem("Hammer", 1) }, findItem.Object, addItem.Object,
            saveProfile.Object, reportFailure.Object);

        Assert.True(firstSpawnHandled);
        Assert.False(secondSpawnHandled);
        findItem.Verify(find => find("Hammer"), Times.Once);
        addItem.Verify(add => add(hammer, 1), Times.Once);
        saveProfile.Verify(save => save(), Times.Once);
        reportFailure.VerifyNoOtherCalls();
    }

    // Verifies that spawning an existing profile neither grants starting items nor saves enrollment.
    [Fact]
    public void ExistingProfileDoesNotReceiveStartingItemsOrEnrollmentSave()
    {
        ClientSaveLifecycle lifecycle = new();
        lifecycle.ActivateExisting();
        Mock<Func<string, object>> findItem = new(MockBehavior.Strict);
        Mock<Func<object, int, bool>> addItem = new(MockBehavior.Strict);
        Mock<Action> saveProfile = new(MockBehavior.Strict);
        Mock<Action<StartingItem>> reportFailure = new(MockBehavior.Strict);

        bool handled = StartingItemGrantPolicy.ApplyEnrollment(lifecycle, true,
            new[] { new StartingItem("Hammer", 1) }, findItem.Object, addItem.Object,
            saveProfile.Object, reportFailure.Object);

        Assert.False(handled);
        findItem.VerifyNoOtherCalls();
        addItem.VerifyNoOtherCalls();
        saveProfile.VerifyNoOtherCalls();
        reportFailure.VerifyNoOtherCalls();
    }
}
