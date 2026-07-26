using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.Moderator
{
    internal static class PlayerPositionRpc
    {
        private const string RequestRpc = "Landoria_Moderator_PositionRequest";
        private const string ResponseRpc = "Landoria_Moderator_PositionResponse";
        private const float RequestTimeout = 5f;
        private static readonly Dictionary<int, PendingRequest> PendingRequests =
            new Dictionary<int, PendingRequest>();
        private static ZRoutedRpc _registeredRpc;
        private static int _nextRequestId;

        internal static void RegisterResponseRpc()
        {
            ZRoutedRpc rpc = ZRoutedRpc.instance;
            if (rpc == null || ReferenceEquals(rpc, _registeredRpc))
            {
                return;
            }

            rpc.Register<int, Vector3>(ResponseRpc, ReceiveResponse);
            _registeredRpc = rpc;
            ModeratorPlugin.ModLogger.LogDebug("Player position response RPC registered.");
        }

        internal static void RegisterPlayerRpc(Player player, ZNetView netView)
        {
            if (netView?.GetZDO() == null)
            {
                return;
            }

            netView.Register<int>(RequestRpc,
                (sender, requestId) => SendResponse(player, sender, requestId));
        }

        internal static bool Request(string playerName, Action<Vector3> callback)
        {
            if (!PlayerCommandUtility.TryFindPlayer(
                    playerName, out _, out ZDOID characterId) || ZRoutedRpc.instance == null)
            {
                return false;
            }

            return Request(characterId, playerName, callback, true);
        }

        internal static bool Request(
            ZDOID characterId, string playerName, Action<Vector3> callback,
            bool logRequest = false)
        {
            if (characterId.IsNone() || ZRoutedRpc.instance == null)
            {
                return false;
            }

            int requestId = ++_nextRequestId;
            PendingRequests[requestId] = new PendingRequest
            {
                Callback = callback,
                ExpiresAt = Time.unscaledTime + RequestTimeout,
                LogResponse = logRequest
            };
            ZRoutedRpc.instance.InvokeRoutedRPC(0L, characterId, RequestRpc, requestId);
            if (logRequest)
            {
                ModeratorPlugin.ModLogger.LogDebug(
                    $"Requested the private position of '{playerName}' with request {requestId}.");
            }
            return true;
        }

        internal static void Update()
        {
            List<int> expired = new List<int>();
            foreach (KeyValuePair<int, PendingRequest> request in PendingRequests)
            {
                if (Time.unscaledTime >= request.Value.ExpiresAt)
                {
                    expired.Add(request.Key);
                }
            }
            foreach (int requestId in expired)
            {
                PendingRequests.Remove(requestId);
            }
        }

        internal static void ResetSession()
        {
            _registeredRpc = null;
            _nextRequestId = 0;
            PendingRequests.Clear();
        }

        private static void SendResponse(Player player, long sender, int requestId)
        {
            if (player == null || ZRoutedRpc.instance == null)
            {
                return;
            }

            ZRoutedRpc.instance.InvokeRoutedRPC(
                sender, ResponseRpc, requestId, player.transform.position);
        }

        private static void ReceiveResponse(long sender, int requestId, Vector3 position)
        {
            if (!PendingRequests.TryGetValue(requestId, out PendingRequest request))
            {
                return;
            }

            PendingRequests.Remove(requestId);
            request.Callback(position);
            if (request.LogResponse)
            {
                ModeratorPlugin.ModLogger.LogDebug(
                    $"Received private player position {position} for request {requestId} from peer {sender}.");
            }
        }

        private sealed class PendingRequest
        {
            internal Action<Vector3> Callback;
            internal float ExpiresAt;
            internal bool LogResponse;
        }
    }
}
