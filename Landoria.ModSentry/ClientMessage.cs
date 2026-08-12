namespace Landoria.ModSentry
{
    internal static class ClientMessage
    {
        private const float DisconnectFallbackSeconds = 2f;
        private static string _pending;
        private static float _disconnectDeadline;
        private static bool _returnToMenu;

        internal static void Receive(ZRpc rpc, string message)
        {
            _pending = message;
            _returnToMenu = true;
            _disconnectDeadline = UnityEngine.Time.unscaledTime + DisconnectFallbackSeconds;
            ModSentryPlugin.Log.LogWarning($"Server rejected the connection: {message}");
            rpc.Invoke(ModSentryPlugin.RejectionAckRpc);
            ModSentryPlugin.Log.LogDebug(
                "Acknowledged the rejection; waiting for the server disconnect before returning to the menu.");
        }

        internal static bool TryGet(out string message)
        {
            message = _pending;
            return !string.IsNullOrWhiteSpace(message);
        }

        internal static void Tick()
        {
            if (!_returnToMenu || !ReadyToReturn())
            {
                return;
            }

            if (Game.instance == null)
            {
                return;
            }

            _returnToMenu = false;
            ZNet.SetExternalError(ZNet.ConnectionStatus.ErrorVersion);
            ModSentryPlugin.Log.LogInfo("Returning to the main menu to display the rejection reason.");
            Game.instance.Logout(false, true);
        }

        internal static void Clear()
        {
            _pending = null;
            _returnToMenu = false;
        }

        private static bool ReadyToReturn()
        {
            ZNet.ConnectionStatus status = ZNet.GetConnectionStatus();
            return status != ZNet.ConnectionStatus.Connecting ||
                   UnityEngine.Time.unscaledTime >= _disconnectDeadline;
        }
    }
}
