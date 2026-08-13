using Xunit;

namespace Landoria.CharacterVault;

public sealed class WorldSaveCharacterCheckpointTests
{
    // Verifies that a server world save requests character checkpoints.
    [Fact]
    public void ServerWorldSaveRequestsCharacterCheckpoint()
    {
        int requests = 0;

        WorldSavePolicy.Handle(isServer: true, () => requests++);

        Assert.Equal(1, requests);
    }
}
