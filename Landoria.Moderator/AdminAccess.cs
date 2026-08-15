using System;

namespace Landoria.Moderator
{
    internal static class AdminAccess
    {
        private const string RequestRpc = "Landoria_Moderator_AdminRequest";
        private const string ResponseRpc = "Landoria_Moderator_AdminResponse";

        private static ZRoutedRpc _registeredRpc;
        private static bool _serverConfirmedAdmin;
        private static bool _requestSent;

        internal static bool LocalPlayerIsAdminOrHost()
        {
            return ZNet.instance != null &&
                   (ZNet.instance.IsServer() || _serverConfirmedAdmin);
        }

        internal static void RegisterRpcs()
        {
            ZRoutedRpc rpc = ZRoutedRpc.instance;
            if (rpc == null || ReferenceEquals(rpc, _registeredRpc))
            {
                return;
            }

            rpc.Register(RequestRpc, ReceiveRequest);
            rpc.Register<bool>(ResponseRpc, ReceiveResponse);
            _registeredRpc = rpc;
            ModeratorPlugin.ModLogger.LogDebug("Administrator validation RPCs registered.");
        }

        internal static void RequestStatus()
        {
            if (ZNet.instance == null || ZNet.instance.IsServer() ||
                ZRoutedRpc.instance == null || _requestSent)
            {
                return;
            }

            _serverConfirmedAdmin = false;
            _requestSent = true;
            ZRoutedRpc.instance.InvokeRoutedRPC(RequestRpc);
            ModeratorPlugin.ModLogger.LogDebug("Requested administrator validation from the server.");
        }

        internal static void ResetSession()
        {
            _registeredRpc = null;
            _serverConfirmedAdmin = false;
            _requestSent = false;
        }

        private static void ReceiveRequest(long sender)
        {
            if (ZNet.instance == null || !ZNet.instance.IsServer() || ZRoutedRpc.instance == null)
            {
                return;
            }

            ZNetPeer peer = ZNet.instance.GetPeer(sender);
            bool isAdmin = peer != null && ZNet.instance.IsAdmin(peer.m_socket.GetHostName());
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, ResponseRpc, isAdmin);
            ModeratorPlugin.ModLogger.LogDebug(
                $"Administrator validation completed for peer {sender}: {isAdmin}.");
        }

        private static void ReceiveResponse(long sender, bool isAdmin)
        {
            if (ZNet.instance == null || ZNet.instance.IsServer() || ZNet.instance.GetPeer(sender) == null)
            {
                return;
            }

            _serverConfirmedAdmin = isAdmin;
            if (!isAdmin)
            {
                ModeratorState.SetEnabled(false);
            }
            ModeratorPlugin.ModLogger.LogDebug(
                $"Server administrator validation received: {isAdmin}.");
        }
    }
}
