using UnityEngine;

namespace Landoria.ModSentry
{
    internal static class PendingDisconnects
    {
        private const float FallbackSeconds = 2f;
        private static readonly PendingDisconnectRegistry<ZRpc> Registry =
            new PendingDisconnectRegistry<ZRpc>(() => Time.unscaledTime, FallbackSeconds);

        internal static void Schedule(ZRpc rpc)
        {
            Registry.Schedule(rpc);
        }

        internal static bool Acknowledge(ZRpc rpc)
        {
            return Registry.Acknowledge(rpc);
        }

        internal static void Remove(ZRpc rpc)
        {
            Registry.Remove(rpc);
        }

        internal static void Tick()
        {
            Registry.Tick(ModSentryHandshake.Disconnect);
        }

        internal static void Clear()
        {
            Registry.Clear();
        }
    }
}
