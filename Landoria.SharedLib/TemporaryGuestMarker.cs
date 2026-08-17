using System.Linq;

namespace Landoria.SharedLib
{
    public static class TemporaryGuestMarker
    {
        private const string Key = "landoria.temporary_guest";
        private const string Value = "1";

        public static void Mark(ZRpc rpc)
        {
            ZNetPeer peer = FindPeer(rpc);
            if (peer != null)
            {
                peer.m_serverSyncedPlayerData[Key] = Value;
            }
        }

        public static bool IsMarked(ZRpc rpc)
        {
            ZNetPeer peer = FindPeer(rpc);
            return peer != null && peer.m_serverSyncedPlayerData.TryGetValue(Key,
                out string value) && value == Value;
        }

        public static bool IsMarked(string hostName)
        {
            return ZNet.instance?.GetPeers().Any(peer => IsHost(peer, hostName) &&
                IsMarked(peer.m_rpc)) == true;
        }

        private static bool IsHost(ZNetPeer peer, string hostName)
        {
            return peer?.m_rpc != null && peer.m_socket?.GetHostName() == hostName;
        }

        private static ZNetPeer FindPeer(ZRpc rpc)
        {
            return ZNet.instance?.GetPeers()
                .FirstOrDefault(peer => ReferenceEquals(peer.m_rpc, rpc));
        }
    }
}
