using System.Collections.Generic;

namespace Landoria.ModSentry
{
    internal sealed class HandshakeRegistry<TPeer>
    {
        private readonly HashSet<TPeer> _accepted = new HashSet<TPeer>();
        private readonly Dictionary<TPeer, ValidationResult> _rejected =
            new Dictionary<TPeer, ValidationResult>();

        internal void Accept(TPeer peer)
        {
            _rejected.Remove(peer);
            _accepted.Add(peer);
        }

        internal void Reject(TPeer peer, ValidationResult result)
        {
            _accepted.Remove(peer);
            _rejected[peer] = result;
        }

        internal bool IsAccepted(TPeer peer)
        {
            return _accepted.Contains(peer);
        }

        internal ValidationResult RejectionFor(TPeer peer)
        {
            return _rejected.TryGetValue(peer, out ValidationResult result) ? result : null;
        }

        internal void Remove(TPeer peer)
        {
            _accepted.Remove(peer);
            _rejected.Remove(peer);
        }

        internal void Clear()
        {
            _accepted.Clear();
            _rejected.Clear();
        }
    }
}
