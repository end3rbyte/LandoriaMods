namespace Landoria.ModSentry
{
    internal static class ClientMessage
    {
        private static readonly ClientRejectionState State = new ClientRejectionState();

        internal static void Receive(ZRpc rpc, string message)
        {
            State.Receive(message);
            ModSentryPlugin.Log.LogWarning($"Server rejected the connection: {message}");
            rpc.Invoke(ModSentryPlugin.RejectionAckRpc);
            ModSentryPlugin.Log.LogDebug(
                "Acknowledged the rejection; waiting for the server disconnect.");
        }

        internal static bool TryGet(out string message)
        {
            return State.TryGet(out message);
        }

        internal static void Clear()
        {
            State.Clear();
        }
    }
}
