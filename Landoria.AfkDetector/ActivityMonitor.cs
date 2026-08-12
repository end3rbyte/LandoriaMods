using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.AfkDetector
{
    internal sealed class ActivityMonitor
    {
        private readonly Dictionary<long, PlayerActivity> _players =
            new Dictionary<long, PlayerActivity>();
        private readonly Action<ZNetPeer> _disconnect;
        private float _timeoutSeconds;
        private float _movementToleranceSquared;

        internal ActivityMonitor(float timeoutSeconds, float movementTolerance,
            Action<ZNetPeer> disconnect)
        {
            _disconnect = disconnect;
            Configure(timeoutSeconds, movementTolerance);
        }

        internal void Configure(float timeoutSeconds, float movementTolerance)
        {
            _timeoutSeconds = timeoutSeconds;
            _movementToleranceSquared = movementTolerance * movementTolerance;
        }

        internal void Update(List<ZNetPeer> peers, float now)
        {
            HashSet<long> connected = new HashSet<long>();
            foreach (ZNetPeer peer in peers)
            {
                if (!peer.IsReady())
                {
                    continue;
                }
                connected.Add(peer.m_uid);
                UpdatePeer(peer, now);
            }
            RemoveDisconnected(connected);
        }

        internal void RecordChat(long peerId, float now)
        {
            if (_players.TryGetValue(peerId, out PlayerActivity activity))
            {
                activity.LastActivityAt = now;
            }
        }

        private void UpdatePeer(ZNetPeer peer, float now)
        {
            if (!_players.TryGetValue(peer.m_uid, out PlayerActivity activity))
            {
                _players[peer.m_uid] = new PlayerActivity(peer.GetRefPos(), now);
                return;
            }
            if (HasMoved(activity.Position, peer.GetRefPos()))
            {
                activity.Position = peer.GetRefPos();
                activity.LastActivityAt = now;
                return;
            }
            if (!activity.DisconnectRequested && now - activity.LastActivityAt >= _timeoutSeconds)
            {
                activity.DisconnectRequested = true;
                _disconnect(peer);
            }
        }

        private bool HasMoved(Vector3 previous, Vector3 current)
        {
            return (current - previous).sqrMagnitude >= _movementToleranceSquared;
        }

        private void RemoveDisconnected(HashSet<long> connected)
        {
            List<long> stale = new List<long>();
            foreach (long peerId in _players.Keys)
            {
                if (!connected.Contains(peerId))
                {
                    stale.Add(peerId);
                }
            }
            foreach (long peerId in stale)
            {
                _players.Remove(peerId);
            }
        }

        private sealed class PlayerActivity
        {
            internal Vector3 Position;
            internal float LastActivityAt;
            internal bool DisconnectRequested;

            internal PlayerActivity(Vector3 position, float now)
            {
                Position = position;
                LastActivityAt = now;
            }
        }
    }
}
