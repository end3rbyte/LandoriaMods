using System;
using System.Collections.Generic;
using System.Linq;

namespace Landoria.ModSentry
{
    internal static class ModSentryHandshake
    {
        internal static void Register(ZNet network, ZNetPeer peer)
        {
            if (network.IsServer())
            {
                peer.m_rpc.Register<ZPackage>(ModSentryPlugin.InventoryRpc, ReceiveInventory);
                peer.m_rpc.Register(ModSentryPlugin.RejectionAckRpc, ReceiveRejectionAck);
            }
            else
            {
                ClientMessage.Clear();
                peer.m_rpc.Register<string>(ModSentryPlugin.RejectionRpc, ClientMessage.Receive);
            }
        }

        internal static void SendInventory(ZRpc serverRpc)
        {
            serverRpc.Invoke(ModSentryPlugin.InventoryRpc, PluginInventory.Serialize());
        }

        internal static void ReceiveInventory(ZRpc rpc, ZPackage package)
        {
            try
            {
                IReadOnlyList<PluginDescriptor> inventory = PluginInventory.Deserialize(package);
                ValidationResult result = PolicyValidator.Validate(
                    ModSentryPlugin.EnsurePolicy(), inventory);
                Record(rpc, result);
            }
            catch (Exception exception)
            {
                ValidationResult result = ValidationResult.Reject(
                    "The installed mods could not be verified.",
                    $"Client inventory parsing failed: {exception}");
                Record(rpc, result);
            }
        }

        internal static bool Admit(ZRpc rpc)
        {
            if (HandshakeState.IsAccepted(rpc))
            {
                return true;
            }

            ValidationResult rejection = HandshakeState.RejectionFor(rpc) ??
                ValidationResult.Reject("Mod verification did not complete. Please try again.",
                    "PeerInfo arrived before an accepted ModSentry inventory.");
            rpc.Invoke(ModSentryPlugin.RejectionRpc, rejection.PlayerMessage);
            ModSentryPlugin.Log.LogWarning(rejection.TechnicalMessage);
            PendingDisconnects.Schedule(rpc);
            return false;
        }

        internal static void Disconnect(ZRpc rpc)
        {
            ZNetPeer peer = ZNet.instance.GetPeers()
                .FirstOrDefault(candidate => ReferenceEquals(candidate.m_rpc, rpc));
            if (peer != null)
            {
                ModSentryPlugin.Log.LogDebug(
                    "Disconnecting the rejected pre-admission peer directly.");
                ZNet.instance.Disconnect(peer);
            }
        }

        private static void ReceiveRejectionAck(ZRpc rpc)
        {
            if (PendingDisconnects.Acknowledge(rpc))
            {
                Disconnect(rpc);
            }
        }

        private static void Record(ZRpc rpc, ValidationResult result)
        {
            if (result.Accepted)
            {
                HandshakeState.Accept(rpc);
                ModSentryPlugin.Log.LogInfo(result.TechnicalMessage);
                return;
            }

            HandshakeState.Reject(rpc, result);
            ModSentryPlugin.Log.LogWarning(result.TechnicalMessage);
        }
    }
}
