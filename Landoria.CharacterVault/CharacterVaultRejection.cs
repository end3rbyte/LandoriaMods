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
        private static readonly HashSet<string> PermittedListRejections =
            new HashSet<string>();
        private static readonly CharacterRejectionMessageState ClientMessage =
            new CharacterRejectionMessageState();

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

        internal static void RecordPermittedListRejection(string hostName)
        {
            PermittedListRejections.Add(hostName);
        }

        internal static void SendPermittedListRejection(ZRpc rpc)
        {
            string hostName = rpc?.GetSocket()?.GetHostName();
            if (hostName != null && PermittedListRejections.Remove(hostName))
            {
                Reject(rpc, CharacterRejectionMessages.PermittedListDenied);
            }
        }

        internal static bool TryGetMessage(out string message)
        {
            return ClientMessage.TryGet(out message);
        }

        internal static void Remove(ZRpc rpc)
        {
            Deadlines.Remove(rpc);
        }

        internal static void Tick()
        {
            DisconnectExpired();
        }

        internal static void Clear()
        {
            Deadlines.Clear();
            PermittedListRejections.Clear();
            ClearClient();
        }

        private static void ReceiveMessage(ZRpc rpc, string message)
        {
            ClientMessage.Receive(message);
            CharacterVaultPlugin.Log.LogWarning($"Server rejected the character: {message}");
            rpc.Invoke(AckRpc);
            CharacterVaultPlugin.Log.LogDebug(
                "Acknowledged the CharacterVault rejection; waiting for the server kick.");
        }

        private static void ReceiveAck(ZRpc rpc)
        {
            if (Deadlines.Remove(rpc))
            {
                Kick(rpc);
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
                Kick(rpc);
            }
        }

        private static void Kick(ZRpc rpc)
        {
            ZNetPeer peer = ZNet.instance?.GetPeers()
                .FirstOrDefault(candidate => ReferenceEquals(candidate.m_rpc, rpc));
            if (peer != null)
            {
                string platformPlayerId = peer.m_socket?.GetHostName();
                if (!string.IsNullOrWhiteSpace(platformPlayerId))
                {
                    CharacterVaultPlugin.Log.LogDebug(
                        "Kicking the rejected pre-spawn peer after delivering the rejection reason.");
                    ZNet.instance.Kick(platformPlayerId);
                }
            }
        }

        private static void ClearClient()
        {
            ClientMessage.Clear();
        }
    }
}
