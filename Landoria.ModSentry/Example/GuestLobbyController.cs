using System.Collections.Generic;
using System.Linq;
using Landoria.ModSentry;
using UnityEngine;

namespace GuestLobbyExample
{
    /// <summary>Handles guest admission, sessions, and teleportation.</summary>
    internal sealed class GuestLobbyController : IUnverifiedGuestController
    {
        private const float RetrySeconds = 0.25f;
        private static readonly Dictionary<ZRpc, GuestState> Guests =
            new Dictionary<ZRpc, GuestState>();

        /// <summary>Gets the ModSentry guest-controller protocol version.</summary>
        public int ProtocolVersion =>
            ModSentryPlugin.GuestControllerProtocolVersion;

        /// <summary>Gets whether the generated lobby can admit guests.</summary>
        public bool IsReady => GuestLobbyGenerator.IsOperational &&
            GuestLobbyGenerator.TryGetPosition(out _);

        /// <summary>Starts tracking an admitted guest connection.</summary>
        public void OnGuestAdmitted(ZRpc rpc)
        {
            if (!IsReady)
            {
                throw new System.InvalidOperationException(
                    "The guest lobby is unavailable.");
            }
            Guests[rpc] = new GuestState();
            GuestLobbyPlugin.Log.LogInfo("Started tracking an admitted guest.");
        }

        /// <summary>Stops tracking a disconnected guest connection.</summary>
        public void OnGuestDisconnected(ZRpc rpc)
        {
            if (Guests.Remove(rpc))
            {
                GuestLobbyPlugin.Log.LogInfo(
                    "Stopped tracking a disconnected guest.");
            }
        }

        /// <summary>Clears all tracked guest sessions.</summary>
        public void ClearGuests()
        {
            if (Guests.Count == 0)
            {
                return;
            }
            Guests.Clear();
            GuestLobbyPlugin.Log.LogInfo("Cleared all tracked guest sessions.");
        }

        /// <summary>Checks whether a connection is an admitted guest.</summary>
        internal static bool IsAdmitted(ZRpc rpc)
        {
            return rpc != null && Guests.ContainsKey(rpc);
        }

        /// <summary>Checks whether at least one guest is inside the lobby.</summary>
        internal static bool HasGuestInside()
        {
            return Guests.Values.Any(state => state.IsInside);
        }

        /// <summary>Updates confinement and protected sign state.</summary>
        internal static void Tick()
        {
            if (!GuestLobbyGenerator.TryGetPosition(out Vector3 lobby))
            {
                return;
            }
            foreach (ZRpc rpc in Guests.Keys.ToArray())
            {
                ConfineWhenReady(rpc, Guests[rpc], lobby);
            }
            GuestLobbyProtection.TickSign();
        }

        private static void ConfineWhenReady(ZRpc rpc, GuestState state,
            Vector3 lobby)
        {
            ZNetPeer peer = FindPeer(rpc);
            if (peer == null || !peer.IsReady() || peer.m_characterID.IsNone())
            {
                return;
            }
            ZDO character = ZDOMan.instance?.GetZDO(peer.m_characterID);
            if (character == null || !character.IsValid())
            {
                return;
            }
            state.IsInside = GuestLobbyProtection.IsInsideLobby(
                character.GetPosition(), lobby);
            if (state.IsInside)
            {
                return;
            }
            SendTeleport(peer, state, lobby);
        }

        private static void SendTeleport(ZNetPeer peer, GuestState state,
            Vector3 lobby)
        {
            float now = Time.unscaledTime;
            if (now < state.NextTeleportAt || ZRoutedRpc.instance == null)
            {
                return;
            }
            state.NextTeleportAt = now + RetrySeconds;
            ZRoutedRpc.instance.InvokeRoutedRPC(0L, peer.m_characterID,
                "RPC_TeleportTo", lobby, Quaternion.identity, true);
        }

        /// <summary>Finds the connected peer for a guest RPC.</summary>
        internal static ZNetPeer FindPeer(ZRpc rpc)
        {
            return ZNet.instance?.GetPeers()
                .FirstOrDefault(peer => ReferenceEquals(peer.m_rpc, rpc));
        }

        private sealed class GuestState
        {
            internal float NextTeleportAt { get; set; }
            internal bool IsInside { get; set; }
        }
    }
}
