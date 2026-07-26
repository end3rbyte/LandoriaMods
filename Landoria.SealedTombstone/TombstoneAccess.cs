using System;

namespace Landoria.SealedTombstone
{
    internal static class TombstoneAccess
    {
        private const string LockDayKey = "Landoria.SealedTombstone.LockDay";
        private const string BlockedPlayersKey = "Landoria.SealedTombstone.BlockedPlayers";
        private const string RequestRpc = "Landoria_SealedTombstone_Request";
        private const string DecisionRpc = "Landoria_SealedTombstone_Decision";
        private const int UnlockAfterDays = 10;
        private const int RequestExpirySeconds = 30;
        private const int RequestCooldownSeconds = 120;

        private static PendingRequest _pendingRequest;
        private static DateTime _lastRequestAt = DateTime.MinValue;
        private static ZRoutedRpc _registeredRpc;

        internal static void RegisterRpcs()
        {
            ZRoutedRpc rpc = ZRoutedRpc.instance;
            if (rpc == null || ReferenceEquals(rpc, _registeredRpc))
            {
                return;
            }

            rpc.Register<long, string, ZDOID, long>(RequestRpc, ReceiveRequest);
            rpc.Register<long, ZDOID, bool, string>(DecisionRpc, ReceiveDecision);
            _registeredRpc = rpc;
            SealedTombstonePlugin.Log.LogDebug("Tombstone access RPCs registered for the current session.");
        }

        internal static void ResetSession()
        {
            _pendingRequest = null;
            _lastRequestAt = DateTime.MinValue;
            _registeredRpc = null;
            RecentAttackers.Reset();
        }

        internal static void RecordLockDay(TombStone tombstone, long ownerId)
        {
            ZNetView view = tombstone.GetComponent<ZNetView>();
            ZDO zdo = view?.GetZDO();
            if (ownerId == 0L || zdo == null || !view.IsOwner())
            {
                return;
            }

            zdo.Set(LockDayKey, CurrentDay());
            zdo.Set(BlockedPlayersKey, RecentAttackers.ConsumeSnapshot());
            SealedTombstonePlugin.Log.LogDebug($"Locked tombstone {zdo.m_uid} for player {ownerId}.");
        }

        internal static bool AllowInteraction(TombStone tombstone, Humanoid character)
        {
            Player player = character as Player;
            ZDO zdo = tombstone.GetComponent<ZNetView>()?.GetZDO();
            if (player == null || zdo == null)
            {
                return true;
            }

            long ownerId = zdo.GetLong(ZDOVars.s_owner, 0L);
            if (ownerId == 0L || ownerId == player.GetPlayerID() || IsExpired(zdo))
            {
                return true;
            }

            if (IsBlocked(zdo, player.GetPlayerID()))
            {
                player.Message(MessageHud.MessageType.Center,
                    "You cannot request access to this tombstone.");
                SealedTombstonePlugin.Log.LogDebug($"Blocked tombstone request from recent attacker {player.GetPlayerID()}.");
                return false;
            }

            RequestAccess(player, ownerId, zdo.m_uid);
            return false;
        }

        internal static void Tick()
        {
            if (_pendingRequest == null || !HasExpired(_pendingRequest))
            {
                return;
            }

            SealedTombstonePlugin.Log.LogDebug($"Request from {_pendingRequest.RequesterPlayerId} expired.");
            ClosePopup();
            SendDecision(_pendingRequest, accepted: false);
            _pendingRequest = null;
        }

        private static void RequestAccess(Player requester, long ownerId, ZDOID tombstoneId)
        {
            if ((DateTime.UtcNow - _lastRequestAt).TotalSeconds < RequestCooldownSeconds)
            {
                requester.Message(MessageHud.MessageType.Center, "Please wait before sending another request.");
                return;
            }

            if (ZRoutedRpc.instance == null)
            {
                requester.Message(MessageHud.MessageType.Center, "The tombstone owner is offline.");
                return;
            }

            _lastRequestAt = DateTime.UtcNow;
            requester.Message(MessageHud.MessageType.Center, "Access request sent to the tombstone owner.");
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RequestRpc,
                requester.GetPlayerID(), requester.GetPlayerName(), tombstoneId, ownerId);
            SealedTombstonePlugin.Log.LogInfo($"Player {requester.GetPlayerID()} requested access to {tombstoneId}.");
        }

        private static void ReceiveRequest(
            long sender,
            long requesterId,
            string requesterName,
            ZDOID tombstoneId,
            long ownerId)
        {
            Player owner = Player.m_localPlayer;
            ZDO tombstone = ZDOMan.instance?.GetZDO(tombstoneId);
            if (owner == null || owner.GetPlayerID() != ownerId || _pendingRequest != null ||
                IsBlocked(tombstone, requesterId))
            {
                return;
            }

            _pendingRequest = new PendingRequest
            {
                RequesterPlayerId = requesterId,
                RequesterName = SafeName(requesterName),
                TombstoneId = tombstoneId,
                CreatedAt = DateTime.UtcNow
            };
            ShowDecisionPopup(_pendingRequest);
        }

        private static void ShowDecisionPopup(PendingRequest request)
        {
            if (!UnifiedPopup.IsAvailable())
            {
                SealedTombstonePlugin.Log.LogWarning("The unlock request was rejected because the VV popup is unavailable.");
                Decide(request, accepted: false);
                return;
            }

            string text = $"Allow {request.RequesterName} to loot this tombstone?";
            UnifiedPopup.Push(new YesNoPopup("Tombstone access request", text,
                () => Decide(request, accepted: true),
                () => Decide(request, accepted: false), localizeText: false));
        }

        private static void Decide(PendingRequest request, bool accepted)
        {
            if (!ReferenceEquals(request, _pendingRequest) || HasExpired(request))
            {
                ClosePopup();
                _pendingRequest = null;
                return;
            }

            ClosePopup();
            _pendingRequest = null;
            SendDecision(request, accepted);
            Player.m_localPlayer?.Message(MessageHud.MessageType.Center,
                accepted ? "Tombstone access granted." : "Tombstone access denied.");
        }

        private static void SendDecision(PendingRequest request, bool accepted)
        {
            Player owner = Player.m_localPlayer;
            if (owner == null || ZRoutedRpc.instance == null)
            {
                return;
            }

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, DecisionRpc,
                request.RequesterPlayerId, request.TombstoneId, accepted, owner.GetPlayerName());
            SealedTombstonePlugin.Log.LogInfo($"Tombstone request from {request.RequesterPlayerId} accepted={accepted}.");
        }

        private static void ReceiveDecision(
            long sender,
            long requesterId,
            ZDOID tombstoneId,
            bool accepted,
            string ownerName)
        {
            Player requester = Player.m_localPlayer;
            if (requester == null || requester.GetPlayerID() != requesterId)
            {
                return;
            }

            if (accepted)
            {
                Unlock(tombstoneId);
            }

            string message = accepted
                ? $"{SafeName(ownerName)} granted access to the tombstone."
                : $"{SafeName(ownerName)} denied or did not answer the request.";
            requester.Message(MessageHud.MessageType.Center, message);
        }

        private static void Unlock(ZDOID tombstoneId)
        {
            ZNetView view = ZNetScene.instance?.FindInstance(tombstoneId)?.GetComponent<ZNetView>();
            if (view == null || !view.IsValid())
            {
                SealedTombstonePlugin.Log.LogWarning($"Cannot unlock unavailable tombstone {tombstoneId}.");
                return;
            }

            view.ClaimOwnership();
            view.GetZDO().Set(ZDOVars.s_owner, 0L);
            SealedTombstonePlugin.Log.LogInfo($"Tombstone {tombstoneId} was unlocked.");
        }

        private static bool IsBlocked(ZDO tombstone, long playerId)
        {
            return tombstone != null && RecentAttackers.Contains(
                tombstone.GetString(BlockedPlayersKey), playerId);
        }

        private static bool IsExpired(ZDO tombstone)
        {
            long lockDay = tombstone.GetLong(LockDayKey, -1L);
            long currentDay = CurrentDay();
            return lockDay >= 0L && currentDay >= 0L && currentDay - lockDay >= UnlockAfterDays;
        }

        private static long CurrentDay()
        {
            return EnvMan.instance != null ? EnvMan.instance.GetDay() : -1L;
        }

        private static bool HasExpired(PendingRequest request)
        {
            return (DateTime.UtcNow - request.CreatedAt).TotalSeconds > RequestExpirySeconds;
        }

        private static string SafeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "A player";
            }

            string safeName = name.Replace("<", string.Empty).Replace(">", string.Empty);
            return safeName.Length <= 64 ? safeName : safeName.Substring(0, 64);
        }

        private static void ClosePopup()
        {
            if (UnifiedPopup.IsAvailable() && UnifiedPopup.IsVisible())
            {
                UnifiedPopup.Pop();
            }
        }
    }
}
