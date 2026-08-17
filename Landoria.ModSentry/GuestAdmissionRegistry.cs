using System;
using System.Collections.Generic;

namespace Landoria.ModSentry
{
    internal sealed class GuestAdmissionRegistry<TPeer>
    {
        private readonly Dictionary<TPeer, GuestAdmissionState> _guests =
            new Dictionary<TPeer, GuestAdmissionState>();
        private readonly Func<float> _time;
        private readonly int _durationSeconds;

        internal GuestAdmissionRegistry(Func<float> time, int durationSeconds)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _durationSeconds = durationSeconds;
        }

        internal void Add(TPeer peer) => _guests[peer] = new GuestAdmissionState();
        internal bool Contains(TPeer peer) => _guests.ContainsKey(peer);
        internal void Remove(TPeer peer) => _guests.Remove(peer);
        internal void Clear() => _guests.Clear();

        internal void Tick(Func<TPeer, bool> ready, Action<TPeer, int, bool> notify,
            Action<TPeer> disconnect)
        {
            foreach (TPeer peer in new List<TPeer>(_guests.Keys))
            {
                TickPeer(peer, ready, notify, disconnect);
            }
        }

        private void TickPeer(TPeer peer, Func<TPeer, bool> ready,
            Action<TPeer, int, bool> notify, Action<TPeer> disconnect)
        {
            GuestAdmissionState state = _guests[peer];
            if (!state.Started && !ready(peer))
            {
                return;
            }
            if (!state.Started)
            {
                state.Start(_time(), _durationSeconds);
            }

            int remaining = Math.Max(0, (int)Math.Ceiling(state.Deadline - _time()));
            if (remaining != state.LastDisplayed)
            {
                notify(peer, remaining, state.LastDisplayed < 0);
                state.LastDisplayed = remaining;
            }
            if (remaining == 0)
            {
                _guests.Remove(peer);
                disconnect(peer);
            }
        }
    }

    internal sealed class GuestAdmissionState
    {
        internal float Deadline { get; private set; }
        internal int LastDisplayed { get; set; } = -1;
        internal bool Started { get; private set; }

        internal void Start(float now, int durationSeconds)
        {
            Deadline = now + durationSeconds;
            Started = true;
        }
    }
}
