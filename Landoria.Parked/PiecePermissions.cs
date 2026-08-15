using System.Collections.Generic;
using HarmonyLib;
using Landoria.Socialize;
using UnityEngine;

namespace Landoria.Parked
{
    internal static class PiecePermissions
    {
        private static readonly Dictionary<long, int> SyncedPlayerGroups =
            new Dictionary<long, int>();
        private static readonly Dictionary<long, long> SyncedPeerPlayers =
            new Dictionary<long, long>();
        private static readonly Dictionary<long, string> SyncedPlayerNames =
            new Dictionary<long, string>();
        private static bool hasServerState;

        internal static void Reset()
        {
            SyncedPlayerGroups.Clear();
            SyncedPeerPlayers.Clear();
            SyncedPlayerNames.Clear();
            hasServerState = false;
        }

        internal static void WriteState(ZPackage package)
        {
            package.Write(GroupState.PlayerGroups.Count);
            foreach (KeyValuePair<long, int> membership in GroupState.PlayerGroups)
            {
                package.Write(membership.Key);
                package.Write(membership.Value);
            }

            List<KeyValuePair<long, long>> connectedPeers = GetConnectedPeers();
            package.Write(connectedPeers.Count);
            foreach (KeyValuePair<long, long> mapping in connectedPeers)
            {
                package.Write(mapping.Key);
                package.Write(mapping.Value);
            }
            Dictionary<long, string> names = GetKnownPlayerNames(connectedPeers);
            package.Write(names.Count);
            foreach (KeyValuePair<long, string> player in names)
            {
                package.Write(player.Key);
                package.Write(player.Value);
            }
        }

        internal static void ReadState(ZPackage package)
        {
            SyncedPlayerGroups.Clear();
            int membershipCount = package.ReadInt();
            for (int index = 0; index < membershipCount; index++)
            {
                SyncedPlayerGroups[package.ReadLong()] = package.ReadInt();
            }

            SyncedPeerPlayers.Clear();
            int peerCount = package.ReadInt();
            for (int index = 0; index < peerCount; index++)
            {
                SyncedPeerPlayers[package.ReadLong()] = package.ReadLong();
            }
            SyncedPlayerNames.Clear();
            int nameCount = package.ReadInt();
            for (int index = 0; index < nameCount; index++)
            {
                SyncedPlayerNames[package.ReadLong()] = package.ReadString();
            }
            hasServerState = true;
        }

        internal static string GetKnownPlayerName(long playerId)
        {
            return SyncedPlayerNames.TryGetValue(playerId, out string name) ? name : null;
        }

        internal static bool CanAccess(long playerId, Piece piece)
        {
            bool placedByPlayer = piece != null && piece.IsPlacedByPlayer();
            long creator = piece != null ? piece.GetCreator() : 0L;
            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                return PieceAccessPolicy.CanAccess(placedByPlayer, playerId, creator,
                    true, AreServerGroupMembers);
            }
            return PieceAccessPolicy.CanAccess(placedByPlayer, playerId, creator,
                hasServerState, AreSyncedGroupMembers);
        }

        private static bool CanAccess(Humanoid humanoid, Piece piece)
        {
            return !(humanoid is Player player) || CanAccess(player.GetPlayerID(), piece);
        }

        private static bool AreServerGroupMembers(long playerId, long creator)
        {
            return GroupState.PlayerGroups.TryGetValue(playerId, out int playerGroup) &&
                   GroupState.PlayerGroups.TryGetValue(creator, out int creatorGroup) &&
                   playerGroup == creatorGroup;
        }

        private static bool AreSyncedGroupMembers(long playerId, long creator)
        {
            return SyncedPlayerGroups.TryGetValue(playerId, out int playerGroup) &&
                   SyncedPlayerGroups.TryGetValue(creator, out int creatorGroup) &&
                   playerGroup == creatorGroup;
        }

        private static long ResolvePeerPlayer(long peer)
        {
            if (peer == ZNet.GetUID() && Game.instance != null)
            {
                return Game.instance.GetPlayerProfile().GetPlayerID();
            }
            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                return GroupState.PeerPlayers.TryGetValue(peer, out long serverPlayer)
                    ? serverPlayer
                    : 0L;
            }
            return SyncedPeerPlayers.TryGetValue(peer, out long syncedPlayer)
                ? syncedPlayer
                : 0L;
        }

        private static List<KeyValuePair<long, long>> GetConnectedPeers()
        {
            List<KeyValuePair<long, long>> peers = new List<KeyValuePair<long, long>>();
            if (ZNet.instance == null)
            {
                return peers;
            }
            foreach (KeyValuePair<long, long> mapping in GroupState.PeerPlayers)
            {
                ZNetPeer peer = ZNet.instance.GetPeer(mapping.Key);
                if (peer != null && peer.IsReady())
                {
                    peers.Add(mapping);
                }
            }
            return peers;
        }

        private static Dictionary<long, string> GetKnownPlayerNames(
            List<KeyValuePair<long, long>> connectedPeers)
        {
            Dictionary<long, string> names = new Dictionary<long, string>();
            foreach (SocialGroup group in GroupState.Groups.Values)
            {
                foreach (KeyValuePair<long, string> member in group.Members)
                {
                    names[member.Key] = member.Value;
                }
            }
            foreach (KeyValuePair<long, long> mapping in connectedPeers)
            {
                ZNetPeer peer = ZNet.instance.GetPeer(mapping.Key);
                if (peer != null)
                {
                    names[mapping.Value] = peer.m_playerName;
                }
            }
            return names;
        }

        private static Piece FindPiece(GameObject target)
        {
            return target != null ? target.GetComponentInParent<Piece>() : null;
        }

        private static bool CanPeerAccess(long peer, Component component)
        {
            return component != null &&
                   CanAccess(ResolvePeerPlayer(peer), component.GetComponent<Piece>());
        }

        [HarmonyPatch(typeof(Player), "Interact")]
        private static class PlayerInteractPatch
        {
            private static bool Prefix(Player __instance, GameObject go)
            {
                return CanAccess(__instance, FindPiece(go));
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UseItem))]
        private static class UseItemPatch
        {
            private static bool Prefix(Humanoid __instance, bool fromInventoryGui)
            {
                return fromInventoryGui ||
                       CanAccess(__instance, FindPiece(__instance.GetHoverObject()));
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.TryUseItemOnInteractable))]
        private static class TryUseItemPatch
        {
            private static bool Prefix(Humanoid __instance, GameObject hoverObject,
                bool fromInventoryGui)
            {
                return fromInventoryGui || CanAccess(__instance, FindPiece(hoverObject));
            }
        }

        [HarmonyPatch(typeof(Player), "Repair")]
        private static class RepairPatch
        {
            private static bool Prefix(Player __instance, Piece repairPiece)
            {
                Piece piece = repairPiece != null ? repairPiece : __instance.GetHoveringPiece();
                return CanAccess(__instance, piece);
            }
        }

        [HarmonyPatch(typeof(Player), "RemovePiece")]
        private static class RemovePiecePatch
        {
            private static bool Prefix(Player __instance, ref bool __result)
            {
                bool hasAccess = CanAccess(__instance, __instance.GetHoveringPiece());
                return PieceInteractionPolicy.CanRemove(hasAccess, ref __result);
            }
        }

        [HarmonyPatch(typeof(WearNTear), "RPC_Repair")]
        private static class RepairRpcPatch
        {
            private static bool Prefix(WearNTear __instance, long sender)
            {
                return CanAccess(ResolvePeerPlayer(sender), __instance.GetComponent<Piece>());
            }
        }

        [HarmonyPatch(typeof(WearNTear), "RPC_Remove")]
        private static class RemoveRpcPatch
        {
            private static bool Prefix(WearNTear __instance, long sender)
            {
                return CanAccess(ResolvePeerPlayer(sender), __instance.GetComponent<Piece>());
            }
        }

        [HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
        private static class DamageRpcPatch
        {
            private static bool Prefix(WearNTear __instance, HitData hit)
            {
                if (hit == null || !hit.HaveAttacker())
                {
                    return true;
                }
                Piece piece = __instance.GetComponent<Piece>();
                Character attacker = hit.GetAttacker();
                if (attacker is Player player)
                {
                    return PieceInteractionPolicy.CanDamage(
                        CanAccess(player.GetPlayerID(), piece));
                }
                if (attacker != null)
                {
                    return CreatureProtectionPolicy.CanDamageBuilding(
                        DecayProtection.GetActivityMultiplier(piece));
                }
                long playerId = ResolvePeerPlayer(hit.m_attacker.UserID);
                return playerId != 0L
                    ? PieceInteractionPolicy.CanDamage(CanAccess(playerId, piece))
                    : CreatureProtectionPolicy.CanDamageBuilding(
                        DecayProtection.GetActivityMultiplier(piece));
            }
        }

        [HarmonyPatch(typeof(Container), "CheckAccess")]
        private static class ContainerAccessPatch
        {
            private static void Postfix(Container __instance, long playerID, ref bool __result)
            {
                Piece piece = __instance.GetComponent<Piece>();
                if (piece != null && piece.IsPlacedByPlayer())
                {
                    __result = CanAccess(playerID, piece);
                }
            }
        }

        [HarmonyPatch(typeof(Door), "RPC_UseDoor")]
        private static class DoorRpcPatch
        {
            private static bool Prefix(Door __instance, long uid)
            {
                return PieceInteractionPolicy.CanUse(CanPeerAccess(uid, __instance));
            }
        }

        [HarmonyPatch(typeof(Fireplace), "RPC_AddFuel")]
        private static class FireplaceFuelRpcPatch
        {
            private static bool Prefix(Fireplace __instance, long sender)
            {
                return CanPeerAccess(sender, __instance);
            }
        }

        [HarmonyPatch(typeof(Fireplace), "RPC_AddFuelAmount")]
        private static class FireplaceFuelAmountRpcPatch
        {
            private static bool Prefix(Fireplace __instance, long sender)
            {
                return CanPeerAccess(sender, __instance);
            }
        }

        [HarmonyPatch(typeof(Smelter), "RPC_AddOre")]
        private static class SmelterOreRpcPatch
        {
            private static bool Prefix(Smelter __instance, long sender)
            {
                return CanPeerAccess(sender, __instance);
            }
        }

        [HarmonyPatch(typeof(Smelter), "RPC_AddFuel")]
        private static class SmelterFuelRpcPatch
        {
            private static bool Prefix(Smelter __instance, long sender)
            {
                return CanPeerAccess(sender, __instance);
            }
        }

        [HarmonyPatch(typeof(Fermenter), "RPC_AddItem")]
        private static class FermenterItemRpcPatch
        {
            private static bool Prefix(Fermenter __instance, long sender)
            {
                return CanPeerAccess(sender, __instance);
            }
        }
    }
}
