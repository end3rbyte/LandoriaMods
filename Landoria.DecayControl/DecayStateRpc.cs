using Landoria.SharedLib;

namespace Landoria.DecayControl
{
    internal static class DecayStateRpc
    {
        private const string RequestRpc = "Landoria_DecayControl_Request";
        private const string ResponseRpc = "Landoria_DecayControl_Response";
        private static ZRoutedRpc registeredRpc;
        private static ZNetPeer serverPeer;
        private static bool requestSent;

        internal static void Update()
        {
            ZRoutedRpc rpc = ZRoutedRpc.instance;
            if (rpc == null || ReferenceEquals(rpc, registeredRpc))
            {
                return;
            }
            rpc.Register(RequestRpc, ReceiveRequest);
            rpc.Register<ZPackage>(ResponseRpc, ReceiveResponse);
            registeredRpc = rpc;
        }

        internal static void RequestOnSpawn()
        {
            Update();
            if (ZNet.instance == null || ZNet.instance.IsServer() ||
                ZRoutedRpc.instance == null || requestSent)
            {
                return;
            }
            serverPeer = ZNet.instance.GetServerPeer();
            if (serverPeer == null)
            {
                return;
            }
            requestSent = true;
            ZRoutedRpc.instance.InvokeRoutedRPC(serverPeer.m_uid, RequestRpc);
        }

        internal static void ResetSession()
        {
            serverPeer = null;
            requestSent = false;
            DecayControlPlugin.Settings?.ResetState();
            DecayProtection.Reset();
        }

        private static void ReceiveRequest(long sender)
        {
            if (!ServerRole.IsDedicatedServer ||
                ZNet.instance.GetPeer(sender) == null || ZRoutedRpc.instance == null)
            {
                return;
            }
            ZPackage package = new ZPackage();
            DecayControlPlugin.Settings.WriteState(package);
            DecayProtection.WriteState(package);
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, ResponseRpc, package);
        }

        private static void ReceiveResponse(long sender, ZPackage package)
        {
            if (ZNet.instance == null || ZNet.instance.IsServer() ||
                serverPeer == null || serverPeer.m_uid != sender)
            {
                return;
            }
            DecayControlPlugin.Settings.ReadState(package);
            DecayProtection.ReadState(package);
        }
    }
}
