namespace Landoria.ModSentry
{
    internal static class HandshakeState
    {
        private static readonly HandshakeRegistry<ZRpc> Registry =
            new HandshakeRegistry<ZRpc>();

        internal static void Accept(ZRpc rpc)
        {
            Registry.Accept(rpc);
        }

        internal static void Reject(ZRpc rpc, ValidationResult result)
        {
            Registry.Reject(rpc, result);
        }

        internal static bool IsAccepted(ZRpc rpc)
        {
            return Registry.IsAccepted(rpc);
        }

        internal static ValidationResult RejectionFor(ZRpc rpc)
        {
            return Registry.RejectionFor(rpc);
        }

        internal static void Remove(ZRpc rpc)
        {
            Registry.Remove(rpc);
        }

        internal static void Clear()
        {
            Registry.Clear();
        }
    }
}
