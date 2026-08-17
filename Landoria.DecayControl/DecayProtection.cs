using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Landoria.SharedLib;
using UnityEngine;

namespace Landoria.DecayControl
{
    internal static class DecayProtection
    {
        private const float VanillaRainDamageFraction = 0.05f;
        private static readonly HashSet<long> ActiveCreators = new HashSet<long>();
        private static ConditionalWeakTable<Fireplace, object> initializedFireplaces =
            new ConditionalWeakTable<Fireplace, object>();
        private static bool hasServerState;

        private struct FireplaceState
        {
            internal float SecondsPerFuel;
            internal bool InfiniteFuel;
        }

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
            return CreatorActivityPolicy.GetActiveCreators(onlinePlayers);
        }

        private static bool IsCreatorActive(long creator, HashSet<long> onlinePlayers)
        {
            return CreatorActivityPolicy.IsCreatorActive(creator, onlinePlayers);
        }

        private static HashSet<long> GetOnlinePlayers()
        {
            HashSet<long> players = new HashSet<long>();
            if (ZNet.instance == null)
            {
                return players;
            }
            foreach (ZNetPeer peer in ZNet.instance.GetPeers())
            {
                if (peer.IsReady() && !peer.m_characterID.IsNone())
                {
                    players.Add(peer.m_characterID.UserID);
                }
            }
            if (!ServerRole.IsDedicatedServer && Game.instance != null)
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
                Piece piece = __instance.GetComponent<Piece>();
                bool isPlayerBuilt = piece != null && piece.IsPlacedByPlayer();
                float activity = GetActivityMultiplier(piece);
                return DecayEffectPolicy.ShouldApplyEnvironmentalWear(isVanillaRainTick,
                    isPlayerBuilt, DecayControlPlugin.Settings.EnvironmentalBuildingWear,
                    activity);
            }
        }

        [HarmonyPatch(typeof(WearNTear), "UpdateWear")]
        private static class NativeWearPatch
        {
            private static void Prefix(WearNTear __instance, out bool __state)
            {
                __state = __instance.m_noRoofWear;
                Piece piece = __instance.GetComponent<Piece>();
                bool isPlayerBuilt = piece != null && piece.IsPlacedByPlayer();
                if (DecayEffectPolicy.ShouldDisableNativeRoofWear(isPlayerBuilt,
                    DecayControlPlugin.Settings.EnvironmentalBuildingWear))
                {
                    __instance.m_noRoofWear = false;
                }
            }

            private static void Postfix(WearNTear __instance, bool __state)
            {
                __instance.m_noRoofWear = __state;
            }
        }

        [HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
        private static class FireplaceFuelPatch
        {
            private static void Prefix(Fireplace __instance, out FireplaceState __state)
            {
                __state = new FireplaceState
                {
                    SecondsPerFuel = __instance.m_secPerFuel,
                    InfiniteFuel = __instance.m_infiniteFuel
                };
                Piece piece = __instance.GetComponent<Piece>();
                bool isPlayerBuilt = piece != null && piece.IsPlacedByPlayer();
                bool firstUpdate = isPlayerBuilt &&
                    !initializedFireplaces.TryGetValue(__instance, out _);
                if (firstUpdate)
                {
                    initializedFireplaces.Add(__instance, new object());
                }
                float activity = GetActivityMultiplier(piece);
                DecayControlMode mode = DecayControlPlugin.Settings.FuelConsumption;
                if (DecayEffectPolicy.ShouldUseNativeInfiniteFuel(isPlayerBuilt, mode))
                {
                    __instance.m_infiniteFuel = true;
                }
                else if (DecayEffectPolicy.ShouldPauseFuel(isPlayerBuilt, firstUpdate,
                    mode, activity))
                {
                    __instance.m_secPerFuel = float.PositiveInfinity;
                }
            }

            private static void Postfix(Fireplace __instance, FireplaceState __state)
            {
                __instance.m_secPerFuel = __state.SecondsPerFuel;
                __instance.m_infiniteFuel = __state.InfiniteFuel;
            }
        }
    }
}
