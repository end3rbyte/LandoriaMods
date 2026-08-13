using System;
using System.Collections.Generic;

namespace Landoria.ModSentry
{
    internal sealed class PendingDisconnectRegistry<TPeer>
    {
        private readonly Dictionary<TPeer, float> _deadlines =
            new Dictionary<TPeer, float>();
        private readonly float _fallbackSeconds;
        private readonly Func<float> _time;

        internal PendingDisconnectRegistry(Func<float> time, float fallbackSeconds)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _fallbackSeconds = fallbackSeconds;
        }

        internal void Schedule(TPeer peer)
        {
            _deadlines[peer] = _time() + _fallbackSeconds;
        }

        internal bool Acknowledge(TPeer peer)
        {
            return _deadlines.Remove(peer);
        }

        internal void Remove(TPeer peer)
        {
            _deadlines.Remove(peer);
        }

        internal void Tick(Action<TPeer> disconnect)
        {
            List<TPeer> expired = new List<TPeer>();
            foreach (KeyValuePair<TPeer, float> pending in _deadlines)
            {
                if (_time() >= pending.Value)
                {
                    expired.Add(pending.Key);
                }
            }

            foreach (TPeer peer in expired)
            {
                _deadlines.Remove(peer);
                disconnect(peer);
            }
        }

        internal void Clear()
        {
            _deadlines.Clear();
        }
    }
}
