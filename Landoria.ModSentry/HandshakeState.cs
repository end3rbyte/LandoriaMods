using System.Collections.Generic;

namespace Landoria.ModSentry
{
    internal static class HandshakeState
    {
        private static readonly HashSet<ZRpc> Accepted = new HashSet<ZRpc>();
        private static readonly Dictionary<ZRpc, ValidationResult> Rejected =
            new Dictionary<ZRpc, ValidationResult>();

        internal static void Accept(ZRpc rpc)
        {
            Rejected.Remove(rpc);
            Accepted.Add(rpc);
        }

        internal static void Reject(ZRpc rpc, ValidationResult result)
        {
            Accepted.Remove(rpc);
            Rejected[rpc] = result;
        }

        internal static bool IsAccepted(ZRpc rpc)
        {
            return Accepted.Contains(rpc);
        }

        internal static ValidationResult RejectionFor(ZRpc rpc)
        {
            return Rejected.TryGetValue(rpc, out ValidationResult result) ? result : null;
        }

        internal static void Remove(ZRpc rpc)
        {
            Accepted.Remove(rpc);
            Rejected.Remove(rpc);
        }

        internal static void Clear()
        {
            Accepted.Clear();
            Rejected.Clear();
        }
    }
}
