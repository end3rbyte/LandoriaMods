namespace Landoria.CharacterVault
{
    internal interface IWorldCheckpointRequest
    {
        void Request();
    }

    internal static class WorldSavePolicy
    {
        internal static void Handle(bool isServer, IWorldCheckpointRequest request)
        {
            if (isServer && request != null)
            {
                request.Request();
            }
        }
    }
}
