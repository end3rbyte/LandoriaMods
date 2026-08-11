using HarmonyLib;
using UnityEngine;

namespace Landoria.SlowDecay
{
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.ApplyDamage))]
    internal static class RainDamagePatch
    {
        private const float VanillaRainDamageFraction = 0.05f;

        private static void Prefix(WearNTear __instance, ref float damage, HitData hitData)
        {
            float rainDamage = __instance.m_health * VanillaRainDamageFraction;
            if (hitData == null && __instance.IsWet() &&
                __instance.GetHealthPercentage() > 0.5f &&
                Mathf.Approximately(damage, rainDamage))
            {
                damage /= SlowDecayPlugin.SlowdownMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
    internal static class FireplaceFuelPatch
    {
        private static void Prefix(Fireplace __instance, out float __state)
        {
            __state = __instance.m_secPerFuel;
            __instance.m_secPerFuel *= SlowDecayPlugin.SlowdownMultiplier;
        }

        private static void Postfix(Fireplace __instance, float __state)
        {
            __instance.m_secPerFuel = __state;
        }
    }
}
