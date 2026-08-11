using System.Collections.Generic;
using UnityEngine;

namespace Landoria.ModSentry
{
    internal static class PendingDisconnects
    {
        private const float FallbackSeconds = 2f;
        private static readonly Dictionary<ZRpc, float> Deadlines =
            new Dictionary<ZRpc, float>();

        internal static void Schedule(ZRpc rpc)
        {
            Deadlines[rpc] = Time.unscaledTime + FallbackSeconds;
        }

        internal static bool Acknowledge(ZRpc rpc)
        {
            return Deadlines.Remove(rpc);
        }

        internal static void Remove(ZRpc rpc)
        {
            Deadlines.Remove(rpc);
        }

        internal static void Tick()
        {
            List<ZRpc> expired = new List<ZRpc>();
            foreach (KeyValuePair<ZRpc, float> pending in Deadlines)
            {
                if (Time.unscaledTime >= pending.Value)
                {
                    expired.Add(pending.Key);
                }
            }
            foreach (ZRpc rpc in expired)
            {
                Deadlines.Remove(rpc);
                ModSentryHandshake.Disconnect(rpc);
            }
        }

        internal static void Clear()
        {
            Deadlines.Clear();
        }
    }
}
