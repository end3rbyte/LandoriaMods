namespace Landoria.ModSentry
{
    internal static class ClientMessage
    {
        private const float DisconnectFallbackSeconds = 2f;
        private static readonly ClientRejectionState State = new ClientRejectionState();

        internal static void Receive(ZRpc rpc, string message)
        {
            State.Receive(message,
                UnityEngine.Time.unscaledTime + DisconnectFallbackSeconds);
            ModSentryPlugin.Log.LogWarning($"Server rejected the connection: {message}");
            rpc.Invoke(ModSentryPlugin.RejectionAckRpc);
            ModSentryPlugin.Log.LogDebug(
                "Acknowledged the rejection; waiting for the server disconnect before returning to the menu.");
        }

        internal static bool TryGet(out string message)
        {
            return State.TryGet(out message);
        }

        internal static void Tick()
        {
            bool connecting = ZNet.GetConnectionStatus() == ZNet.ConnectionStatus.Connecting;
            if (!State.TryBeginReturnToMenu(connecting, UnityEngine.Time.unscaledTime))
            {
                return;
            }

            if (Game.instance == null)
            {
                return;
            }

            ZNet.SetExternalError(ZNet.ConnectionStatus.ErrorVersion);
            ModSentryPlugin.Log.LogInfo("Returning to the main menu to display the rejection reason.");
            Game.instance.Logout(false, true);
        }

        internal static void Clear()
        {
            State.Clear();
        }
    }
}
