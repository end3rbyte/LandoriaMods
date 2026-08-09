using HarmonyLib;
using TMPro;

namespace Landoria.AfkDetector
{
    [HarmonyPatch(typeof(ZNet), "OnNewConnection")]
    internal static class ClientConnectionPatch
    {
        private static void Postfix(ZNet __instance, ZNetPeer peer)
        {
            if (!__instance.IsServer())
            {
                peer.m_rpc.Register<string>(AfkDetectorPlugin.DisconnectReasonRpc,
                    ClientDisconnectReason.Receive);
            }
        }
    }

    [HarmonyPatch(typeof(FejdStartup), "ShowConnectError")]
    internal static class ConnectionErrorPatch
    {
        private static void Postfix(ZNet.ConnectionStatus statusOverride,
            TMP_Text ___m_connectionFailedError)
        {
            ZNet.ConnectionStatus status = statusOverride == ZNet.ConnectionStatus.None
                ? ZNet.GetConnectionStatus()
                : statusOverride;
            if (status == ZNet.ConnectionStatus.ErrorKicked &&
                ClientDisconnectReason.TryTake(out string message))
            {
                ___m_connectionFailedError.text = message;
            }
        }
    }
}
