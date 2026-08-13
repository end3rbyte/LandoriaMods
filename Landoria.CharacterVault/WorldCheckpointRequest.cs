namespace Landoria.CharacterVault
{
    internal sealed class WorldCheckpointRequest : IWorldCheckpointRequest
    {
        private readonly ProfileTransferService _transfers;

        internal WorldCheckpointRequest(ProfileTransferService transfers)
        {
            _transfers = transfers;
        }

        public void Request()
        {
            _transfers.RequestWorldCheckpoint();
        }
    }
}
