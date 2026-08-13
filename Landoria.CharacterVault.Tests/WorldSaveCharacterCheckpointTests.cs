using Moq;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class WorldSaveCharacterCheckpointTests
{
    // Verifies that a server world save requests character checkpoints for connected players.
    [Fact]
    public void ServerWorldSaveRequestsCharacterCheckpoint()
    {
        Mock<IWorldCheckpointRequest> checkpoint = new(MockBehavior.Strict);
        checkpoint
            .Setup(request => request.Request());

        WorldSavePolicy.Handle(isServer: true, checkpoint.Object);

        checkpoint.Verify(request => request.Request(), Times.Once);
    }
}
