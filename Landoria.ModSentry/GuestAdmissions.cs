using System.Collections.Generic;
using System.Linq;

namespace Landoria.ModSentry
{
    public static class GuestAdmissions
    {
        private static readonly HashSet<ZRpc> Guests = new HashSet<ZRpc>();

        internal static bool TryAdd(ZRpc rpc, out string failure)
        {
            failure = null;
            ModSentryGuestMarker.Mark(rpc);
            Guests.Add(rpc);
            if (UnverifiedGuestControllerRegistry.NotifyAdmitted(rpc, out failure))
            {
                return true;
            }
            Guests.Remove(rpc);
            ModSentryGuestMarker.Unmark(rpc);
            return false;
        }

        public static bool IsGuest(ZRpc rpc) => rpc != null && Guests.Contains(rpc);

        public static bool IsGuest(string hostName)
        {
            return ZNet.instance?.GetPeers().Any(peer => peer?.m_rpc != null &&
                peer.m_socket?.GetHostName() == hostName && IsGuest(peer.m_rpc)) == true;
        }

        internal static void Remove(ZRpc rpc)
        {
            if (!Guests.Remove(rpc))
            {
                return;
            }
            UnverifiedGuestControllerRegistry.NotifyDisconnected(rpc);
            ModSentryGuestMarker.Unmark(rpc);
        }

        internal static void Clear()
        {
            foreach (ZRpc rpc in Guests.ToArray())
            {
                Remove(rpc);
            }
            Guests.Clear();
        }
    }
}
