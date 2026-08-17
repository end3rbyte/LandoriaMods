using System.Linq;
using UnityEngine;

namespace Landoria.ModSentry
{
    public static class GuestAdmissions
    {
        private const int DurationSeconds = 30;
        private static readonly GuestAdmissionRegistry<ZRpc> Registry =
            new GuestAdmissionRegistry<ZRpc>(() => Time.unscaledTime, DurationSeconds);

        internal static void Add(ZRpc rpc) => Registry.Add(rpc);
        public static bool IsGuest(ZRpc rpc) => rpc != null && Registry.Contains(rpc);

        public static bool IsGuest(string hostName)
        {
            return ZNet.instance?.GetPeers().Any(peer => peer?.m_rpc != null &&
                peer.m_socket?.GetHostName() == hostName && IsGuest(peer.m_rpc)) == true;
        }
        internal static void Remove(ZRpc rpc) => Registry.Remove(rpc);
        internal static void Clear() => Registry.Clear();

        internal static void Tick()
        {
            Registry.Tick(IsPlayerReady, Notify, Disconnect);
        }

        private static void Disconnect(ZRpc rpc)
        {
            ZNetPeer peer = FindPeer(rpc);
            ModSentryPlugin.Log.LogInfo(
                $"Registration grace period expired for temporary guest " +
                $"{ModSentryHandshake.Describe(peer)}; disconnecting without persistence.");
            ModSentryHandshake.Disconnect(rpc);
        }

        private static bool IsPlayerReady(ZRpc rpc) => FindPlayer(rpc) != null;

        private static void Notify(ZRpc rpc, int seconds, bool first)
        {
            ZNetPeer peer = FindPeer(rpc);
            Player player = FindPlayer(peer);
            if (player == null)
            {
                return;
            }

            string message = ModSentryPlugin.GuestMessage.Value;
            if (first)
            {
                ModSentryPlugin.Log.LogInfo(
                    $"Started the {DurationSeconds}-second registration grace period for " +
                    $"temporary guest {ModSentryHandshake.Describe(peer)}.");
            }
            player.Message(MessageHud.MessageType.Center,
                GuestAdmissionMessages.Countdown(message, seconds));
            if (first && ZRoutedRpc.instance != null)
            {
                SendChat(peer, player, message);
                ModSentryPlugin.Log.LogDebug(
                    "Sent the registration message through chat and the center-screen countdown.");
            }
        }

        private static void SendChat(ZNetPeer peer, Player player, string message)
        {
            UserInfo sender = new UserInfo { Name = GuestAdmissionMessages.Sender };
            ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "ChatMessage",
                player.GetCenterPoint(), (int)Talker.Type.Shout, sender, message);
        }

        private static Player FindPlayer(ZRpc rpc) => FindPlayer(FindPeer(rpc));

        private static Player FindPlayer(ZNetPeer peer)
        {
            return peer == null || peer.m_characterID == ZDOID.None
                ? null : Player.GetPlayer(peer.m_characterID.UserID);
        }

        private static ZNetPeer FindPeer(ZRpc rpc)
        {
            return ZNet.instance?.GetPeers()
                .FirstOrDefault(peer => ReferenceEquals(peer.m_rpc, rpc));
        }
    }
}
