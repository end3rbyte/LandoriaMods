namespace Landoria.ModSentry
{
    internal static class ClientMessage
    {
        private static string _pending;

        internal static void Receive(ZRpc rpc, string message)
        {
            _pending = message;
            rpc.Invoke(ModSentryPlugin.RejectionAckRpc);
        }

        internal static bool TryTake(out string message)
        {
            message = _pending;
            _pending = null;
            return !string.IsNullOrWhiteSpace(message);
        }

        internal static void Clear()
        {
            _pending = null;
        }
    }
}
