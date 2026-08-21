using System;
using System.Collections.Generic;
using System.Linq;
using Landoria.SharedLib;

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

            ValidationResult rejection = HandshakeState.RejectionFor(rpc);
            string failure = null;
            if (rejection == null && UnverifiedGuestControllerRegistry.IsReady &&
                GuestAdmissions.TryAdd(rpc, out failure))
            {
                ModSentryPlugin.Log.LogWarning(
                    "Admitting a client without a ModSentry inventory as a guest.");
                return true;
            }
            if (rejection == null && !string.IsNullOrEmpty(failure))
            {
                ModSentryPlugin.Log.LogError(
                    $"The server-only guest controller rejected admission: {failure}");
            }
            if (rejection == null)
            {
                LogUnavailableGuestAdmission();
            }
            rejection = rejection ?? ValidationResult.Reject(
                "Mod verification did not complete. Please try again.",
                "PeerInfo arrived before an accepted ModSentry inventory.");
            rpc.Invoke(ModSentryPlugin.RejectionRpc, rejection.PlayerMessage);
            ModSentryPlugin.Log.LogWarning(rejection.TechnicalMessage);
            PendingDisconnects.Schedule(rpc);
            return false;
        }

        private static void LogUnavailableGuestAdmission()
        {
            string reason = !UnverifiedGuestControllerRegistry.IsRegistered
                ? "the server-only guest controller is not registered"
                : !UnverifiedGuestControllerRegistry.IsReady
                    ? "the server-only guest controller is not ready"
                : "the server-only guest controller rejected admission";
            ModSentryPlugin.Log.LogWarning(
                $"Rejecting a client without a ModSentry inventory because {reason}.");
        }

        internal static void Disconnect(ZRpc rpc)
        {
            ZNetPeer peer = ZNet.instance.GetPeers()
                .FirstOrDefault(candidate => ReferenceEquals(candidate.m_rpc, rpc));
            if (peer?.m_rpc != null)
            {
                ModSentryPlugin.Log.LogDebug(
                    "Disconnecting the rejected pre-spawn peer after delivering the rejection reason.");
                peer.m_rpc.Invoke("Disconnect");
            }
        }

        internal static string Describe(ZNetPeer peer)
        {
            return string.IsNullOrWhiteSpace(peer?.m_playerName)
                ? "with an unavailable player name" : $"'{peer.m_playerName}'";
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
                VerifiedModpackMarker.Mark(rpc);
                ModSentryPlugin.Log.LogInfo(result.TechnicalMessage);
                return;
            }

            HandshakeState.Reject(rpc, result);
            ModSentryPlugin.Log.LogWarning(result.TechnicalMessage);
        }
    }
}
