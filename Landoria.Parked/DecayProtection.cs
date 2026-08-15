using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Landoria.Socialize;
using UnityEngine;

namespace Landoria.Parked
{
    internal static class DecayProtection
    {
        private const float VanillaRainDamageFraction = 0.05f;
        private static readonly HashSet<long> ActiveCreators = new HashSet<long>();
        private static ConditionalWeakTable<Fireplace, object> initializedFireplaces =
            new ConditionalWeakTable<Fireplace, object>();
        private static bool hasServerState;

        internal static void Reset()
        {
            ActiveCreators.Clear();
            hasServerState = false;
            initializedFireplaces = new ConditionalWeakTable<Fireplace, object>();
        }

        internal static void WriteState(ZPackage package)
        {
            HashSet<long> activeCreators = GetActiveCreators();
            package.Write(activeCreators.Count);
            foreach (long playerId in activeCreators)
            {
                package.Write(playerId);
            }
        }

        internal static void ReadState(ZPackage package)
        {
            ActiveCreators.Clear();
            int count = package.ReadInt();
            for (int index = 0; index < count; index++)
            {
                ActiveCreators.Add(package.ReadLong());
            }
            hasServerState = true;
        }

        internal static float GetActivityMultiplier(Piece piece)
        {
            if (piece == null || !piece.IsPlacedByPlayer())
            {
                return 1f;
            }
            long creator = piece.GetCreator();
            if (creator == 0L)
            {
                return 1f;
            }
            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                return IsCreatorActive(creator, GetOnlinePlayers()) ? 1f : 0f;
            }
            return !hasServerState || ActiveCreators.Contains(creator) ? 1f : 0f;
        }

        private static HashSet<long> GetActiveCreators()
        {
            HashSet<long> onlinePlayers = GetOnlinePlayers();
            return CreatorActivityPolicy.GetActiveCreators(
                onlinePlayers, GroupState.Groups.Values);
        }

        private static bool IsCreatorActive(long creator, HashSet<long> onlinePlayers)
        {
            SocialGroup group = GroupState.GetGroup(creator);
            return CreatorActivityPolicy.IsCreatorActive(creator, onlinePlayers, group);
        }

        private static HashSet<long> GetOnlinePlayers()
        {
            HashSet<long> players = new HashSet<long>();
            if (ZNet.instance == null)
            {
                return players;
            }
            foreach (KeyValuePair<long, long> mapping in GroupState.PeerPlayers)
            {
                ZNetPeer peer = ZNet.instance.GetPeer(mapping.Key);
                if (peer != null && peer.IsReady())
                {
                    players.Add(mapping.Value);
                }
            }
            if (!ZNet.instance.IsDedicated() && Game.instance != null)
            {
                players.Add(Game.instance.GetPlayerProfile().GetPlayerID());
            }
            return players;
        }

        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.ApplyDamage))]
        private static class RainDamagePatch
        {
            private static bool Prefix(WearNTear __instance, float damage, HitData hitData)
            {
                float rainDamage = __instance.m_health * VanillaRainDamageFraction;
                bool isVanillaRainTick = hitData == null && __instance.IsWet() &&
                    __instance.GetHealthPercentage() > 0.5f &&
                    Mathf.Approximately(damage, rainDamage);
                float activity = GetActivityMultiplier(__instance.GetComponent<Piece>());
                return DecayEffectPolicy.ShouldApplyRainDamage(
                    isVanillaRainTick, activity);
            }
        }

        [HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
        private static class FireplaceFuelPatch
        {
            private static void Prefix(Fireplace __instance, out float __state)
            {
                __state = __instance.m_secPerFuel;
                Piece piece = __instance.GetComponent<Piece>();
                bool firstUpdate = piece != null && piece.IsPlacedByPlayer() &&
                    !initializedFireplaces.TryGetValue(__instance, out _);
                if (firstUpdate)
                {
                    initializedFireplaces.Add(__instance, new object());
                }
                float activity = GetActivityMultiplier(piece);
                if (DecayEffectPolicy.ShouldPauseFuel(firstUpdate, activity))
                {
                    __instance.m_secPerFuel = float.PositiveInfinity;
                }
            }

            private static void Postfix(Fireplace __instance, float __state)
            {
                __instance.m_secPerFuel = __state;
            }
        }
    }
}
