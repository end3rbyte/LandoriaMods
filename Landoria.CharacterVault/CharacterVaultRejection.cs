using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Landoria.CharacterVault
{
    internal static class CharacterVaultRejection
    {
        internal const string MessageRpc = "CharacterVault_Rejection_v1";
        internal const string AckRpc = "CharacterVault_RejectionAck_v1";
        private const float DisconnectFallbackSeconds = 2f;
        private static readonly Dictionary<ZRpc, float> Deadlines =
            new Dictionary<ZRpc, float>();
        private static string _clientMessage;
        private static float _clientDeadline;
        private static bool _returnToMenu;

        internal static void RegisterServer(ZRpc rpc)
        {
            rpc.Register(AckRpc, ReceiveAck);
        }

        internal static void RegisterClient(ZRpc rpc)
        {
            ClearClient();
            rpc.Register<string>(MessageRpc, ReceiveMessage);
        }

        internal static void Reject(ZRpc rpc, string message)
        {
            CharacterVaultPlugin.Log.LogWarning(
                $"CharacterVault rejected {rpc.GetSocket().GetHostName()}: {message}");
            Deadlines[rpc] = Time.unscaledTime + DisconnectFallbackSeconds;
            rpc.Invoke(MessageRpc, message);
        }

        internal static bool TryGetMessage(out string message)
        {
            message = _clientMessage;
            return !string.IsNullOrWhiteSpace(message);
        }

        internal static void Remove(ZRpc rpc)
        {
            Deadlines.Remove(rpc);
        }

        internal static void Tick()
        {
            DisconnectExpired();
            ReturnClientToMenu();
        }

        internal static void Clear()
        {
            Deadlines.Clear();
            ClearClient();
        }

        private static void ReceiveMessage(ZRpc rpc, string message)
        {
            _clientMessage = message;
            _returnToMenu = true;
            _clientDeadline = Time.unscaledTime + DisconnectFallbackSeconds;
            CharacterVaultPlugin.Log.LogWarning($"Server rejected the character: {message}");
            rpc.Invoke(AckRpc);
            CharacterVaultPlugin.Log.LogDebug("Acknowledged the CharacterVault rejection.");
        }

        private static void ReceiveAck(ZRpc rpc)
        {
            if (Deadlines.Remove(rpc))
            {
                Disconnect(rpc);
            }
        }

        private static void DisconnectExpired()
        {
            ZRpc[] expired = Deadlines
                .Where(entry => Time.unscaledTime >= entry.Value)
                .Select(entry => entry.Key)
                .ToArray();
            foreach (ZRpc rpc in expired)
            {
                Deadlines.Remove(rpc);
                Disconnect(rpc);
            }
        }

        private static void Disconnect(ZRpc rpc)
        {
            ZNetPeer peer = ZNet.instance?.GetPeers()
                .FirstOrDefault(candidate => ReferenceEquals(candidate.m_rpc, rpc));
            if (peer != null)
            {
                ZNet.instance.Disconnect(peer);
            }
        }

        private static void ReturnClientToMenu()
        {
            if (!_returnToMenu || Game.instance == null || !ClientCanLeave())
            {
                return;
            }

            _returnToMenu = false;
            ZNet.SetExternalError(ZNet.ConnectionStatus.ErrorConnectFailed);
            CharacterVaultPlugin.Log.LogInfo(
                "Returning to the main menu to display the CharacterVault rejection reason.");
            Game.instance.Logout(false, true);
        }

        private static bool ClientCanLeave()
        {
            return ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.Connecting ||
                   Time.unscaledTime >= _clientDeadline;
        }

        private static void ClearClient()
        {
            _clientMessage = null;
            _returnToMenu = false;
        }
    }
}
