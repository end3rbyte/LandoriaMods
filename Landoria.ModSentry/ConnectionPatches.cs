using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.ModSentry
{
    [HarmonyPatch(typeof(ZNet), "OnNewConnection")]
    internal static class RegisterHandshakePatch
    {
        private static void Postfix(ZNet __instance, ZNetPeer peer)
        {
            ModSentryHandshake.Register(__instance, peer);
        }
    }

    [HarmonyPatch(typeof(ZNet), "SendPeerInfo")]
    internal static class SendInventoryPatch
    {
        private static void Prefix(ZRpc rpc)
        {
            if (ZNet.instance != null && !ZNet.instance.IsServer())
            {
                ModSentryHandshake.SendInventory(rpc);
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
    internal static class ValidatePeerPatch
    {
        private static bool Prefix(ZRpc rpc)
        {
            return ZNet.instance == null || !ZNet.instance.IsServer() ||
                   ModSentryHandshake.Admit(rpc);
        }
    }

    [HarmonyPatch(typeof(ZNet), "Disconnect")]
    internal static class ClearHandshakePatch
    {
        private static void Prefix(ZNetPeer peer)
        {
            if (peer?.m_rpc != null)
            {
                HandshakeState.Remove(peer.m_rpc);
                PendingDisconnects.Remove(peer.m_rpc);
                GuestAdmissions.Remove(peer.m_rpc);
            }
        }
    }

    [HarmonyPatch(typeof(FejdStartup), "ShowConnectError")]
    internal static class ShowRejectionPatch
    {
        private static void Postfix(TMP_Text ___m_connectionFailedError)
        {
            if (ClientMessage.TryGet(out string message))
            {
                ___m_connectionFailedError.text = message;
            }
        }
    }

    [HarmonyPatch(typeof(FejdStartup), "Start")]
    internal static class ShowRejectionAfterMenuLoadPatch
    {
        private static void Postfix(
            GameObject ___m_connectionFailedPanel,
            TMP_Text ___m_connectionFailedError)
        {
            if (ClientMessage.TryGet(out string message))
            {
                ___m_connectionFailedError.text = message;
                ___m_connectionFailedPanel.SetActive(true);
            }
        }
    }
}
