using System;
using Landoria.SharedLib;

namespace Landoria.FreeFlyCommand
{
    internal static class FreeFlyAuthorization
    {
        private const string RequestRpc = "Landoria_FreeFlyCommand_Request";
        private const string ResponseRpc = "Landoria_FreeFlyCommand_Response";
        private static ZRoutedRpc _registeredRpc;
        private static ZNetPeer _serverPeer;
        private static bool _requestSent;
        internal static bool IsAuthorized { get; private set; }

        internal static void Update()
        {
            RegisterRpcs();
            if (ZNet.instance == null || ZRoutedRpc.instance == null)
            {
                ResetConnection();
                return;
            }

            if (ZNet.instance.IsServer())
            {
                UpdateServerAuthorization();
            }
            else
            {
                UpdateClientAuthorization();
            }
        }

        internal static void ResetSession()
        {
            ResetConnection();
        }

        internal static void RequestOnSpawn()
        {
            RegisterRpcs();
            if (ZNet.instance == null || ZNet.instance.IsServer() ||
                ZRoutedRpc.instance == null || _requestSent)
            {
                return;
            }

            _serverPeer = ZNet.instance.GetServerPeer();
            if (_serverPeer == null)
            {
                return;
            }

            _requestSent = true;
            ZRoutedRpc.instance.InvokeRoutedRPC(_serverPeer.m_uid, RequestRpc);
        }

        private static void RegisterRpcs()
        {
            ZRoutedRpc rpc = ZRoutedRpc.instance;
            if (rpc == null || ReferenceEquals(rpc, _registeredRpc))
            {
                return;
            }

            rpc.Register(RequestRpc, ReceiveRequest);
            rpc.Register<bool>(ResponseRpc, ReceiveResponse);
            _registeredRpc = rpc;
        }

        private static void UpdateServerAuthorization()
        {
            if (!ServerRole.IsDedicatedServer)
            {
                SetAuthorized(false);
                return;
            }
            FreeFlyCommandPlugin.InitializeDedicatedServerSettings();
            bool allowed = FreeFlyCommandPlugin.ServerEnabled;
            SetAuthorized(allowed);
        }

        private static void UpdateClientAuthorization()
        {
            if (!IsAuthorized)
            {
                FreeFlyController.Disable();
            }

            ZNetPeer currentServer = ZNet.instance.GetServerPeer();
            if (!ReferenceEquals(currentServer, _serverPeer))
            {
                ResetConnection();
                _serverPeer = currentServer;
            }

        }

        private static void ReceiveRequest(long sender)
        {
            if (!ServerRole.IsDedicatedServer ||
                ZNet.instance.GetPeer(sender) == null || ZRoutedRpc.instance == null)
            {
                return;
            }

            ZRoutedRpc.instance.InvokeRoutedRPC(sender, ResponseRpc,
                FreeFlyCommandPlugin.ServerEnabled);
        }

        private static void ReceiveResponse(long sender, bool allowed)
        {
            if (ZNet.instance == null || ZNet.instance.IsServer() ||
                _serverPeer == null || _serverPeer.m_uid != sender)
            {
                return;
            }

            SetAuthorized(allowed);
        }

        private static void ResetConnection()
        {
            _serverPeer = null;
            _requestSent = false;
            FreeFlyController.Disable();
            SetAuthorized(false);
        }

        private static void SetAuthorized(bool allowed)
        {
            if (IsAuthorized == allowed)
            {
                return;
            }

            IsAuthorized = allowed;
            if (!allowed)
            {
                FreeFlyController.Disable();
            }
            FreeFlyCommandPlugin.ModLogger?.LogInfo($"Free-camera authorization is now {allowed}.");
        }
    }
}
