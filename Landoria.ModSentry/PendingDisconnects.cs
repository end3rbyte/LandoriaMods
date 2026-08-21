using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Landoria.ModSentry
{
    internal static class PendingDisconnects
    {
        private const float FallbackSeconds = 2f;
        private static readonly Dictionary<ZRpc, float> Deadlines =
            new Dictionary<ZRpc, float>();
        private static readonly HashSet<ZRpc> DisconnectRequested =
            new HashSet<ZRpc>();

        internal static void Schedule(ZRpc rpc)
        {
            Deadlines[rpc] = Time.unscaledTime + FallbackSeconds;
        }

        internal static void Acknowledge(ZRpc rpc)
        {
            if (Deadlines.ContainsKey(rpc))
            {
                RequestDisconnect(rpc);
            }
        }

        internal static void Remove(ZRpc rpc)
        {
            Deadlines.Remove(rpc);
            DisconnectRequested.Remove(rpc);
        }

        internal static void Tick()
        {
            ZRpc[] expired = Deadlines
                .Where(entry => Time.unscaledTime >= entry.Value)
                .Select(entry => entry.Key)
                .ToArray();
            foreach (ZRpc rpc in expired)
            {
                AdvanceDisconnect(rpc);
            }
        }

        internal static void Clear()
        {
            Deadlines.Clear();
            DisconnectRequested.Clear();
        }

        private static void AdvanceDisconnect(ZRpc rpc)
        {
            if (DisconnectRequested.Contains(rpc))
            {
                Remove(rpc);
                ModSentryHandshake.ForceDisconnect(rpc);
                return;
            }

            RequestDisconnect(rpc);
        }

        private static void RequestDisconnect(ZRpc rpc)
        {
            DisconnectRequested.Add(rpc);
            Deadlines[rpc] = Time.unscaledTime + FallbackSeconds;
            ModSentryHandshake.RequestDisconnect(rpc);
        }
    }
}
