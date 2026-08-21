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
        private const float WelcomeDelaySeconds = 6f;
        private const float GuestDurationSeconds = 15f * 60f;
        private const float ForcedDisconnectDelaySeconds = 2f;
        private const string WelcomeMessage = "Welcome to the Guest Lobby";
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
            Guests[rpc] = new GuestState(Time.unscaledTime);
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
                GuestState state = Guests[rpc];
                if (!DisconnectWhenExpired(rpc, state))
                {
                    ConfineWhenReady(rpc, state, lobby);
                }
            }
            GuestLobbyProtection.TickSign();
        }

        private static bool DisconnectWhenExpired(ZRpc rpc, GuestState state)
        {
            float now = Time.unscaledTime;
            if (!state.DisconnectSent && now >= state.DisconnectAt)
            {
                state.SendDisconnect(rpc, now);
                return true;
            }
            if (!state.DisconnectSent || now < state.ForceDisconnectAt)
            {
                return state.DisconnectSent;
            }
            ZNetPeer peer = FindPeer(rpc);
            if (peer != null)
            {
                GuestLobbyPlugin.Log.LogWarning(
                    "Guest did not disconnect; closing the server connection.");
                ZNet.instance?.Disconnect(peer);
            }
            Guests.Remove(rpc);
            return true;
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
            bool isInside = GuestLobbyProtection.IsInsideLobby(
                character.GetPosition(), lobby);
            if (isInside)
            {
                state.IsInside = true;
                state.ConfirmArrival();
                state.ShowWelcomeWhenReady(peer);
                return;
            }
            if (state.IsInside)
            {
                state.ResetArrival();
            }
            state.IsInside = false;
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
            private bool _arrivalConfirmed;
            private bool _welcomeSent;
            private float _welcomeAt;

            internal GuestState(float admittedAt)
            {
                DisconnectAt = admittedAt + GuestDurationSeconds;
            }

            internal float NextTeleportAt { get; set; }
            internal bool IsInside { get; set; }
            internal float DisconnectAt { get; }
            internal bool DisconnectSent { get; private set; }
            internal float ForceDisconnectAt { get; private set; }

            internal void SendDisconnect(ZRpc rpc, float now)
            {
                DisconnectSent = true;
                ForceDisconnectAt = now + ForcedDisconnectDelaySeconds;
                GuestLobbyPlugin.Log.LogInfo(
                    "Guest session expired; requesting client disconnection.");
                rpc?.Invoke("Disconnect");
            }

            internal void ConfirmArrival()
            {
                if (_arrivalConfirmed)
                {
                    return;
                }
                _arrivalConfirmed = true;
                _welcomeAt = Time.unscaledTime + WelcomeDelaySeconds;
            }

            internal void ResetArrival()
            {
                _arrivalConfirmed = false;
                _welcomeSent = false;
                _welcomeAt = 0f;
            }

            internal void ShowWelcomeWhenReady(ZNetPeer peer)
            {
                if (!_arrivalConfirmed || _welcomeSent ||
                    Time.unscaledTime < _welcomeAt)
                {
                    return;
                }
                _welcomeSent = true;
                GuestLobbyUtility.ShowCenterMessage(peer, WelcomeMessage);
            }
        }
    }
}
