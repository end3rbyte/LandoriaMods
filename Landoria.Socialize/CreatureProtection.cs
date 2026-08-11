using System;
using HarmonyLib;

namespace Landoria.Socialize
{
    internal static class CreatureProtection
    {
        private static bool IsProtected(StaticTarget target)
        {
            Piece piece = target?.GetComponentInParent<Piece>();
            return DecayProtection.GetActivityMultiplier(piece) <= 0f;
        }

        [HarmonyPatch(typeof(StaticTarget), nameof(StaticTarget.IsPriorityTarget))]
        private static class PriorityTargetPatch
        {
            private static void Postfix(StaticTarget __instance, ref bool __result)
            {
                if (__result && IsProtected(__instance))
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(StaticTarget), nameof(StaticTarget.IsRandomTarget))]
        private static class RandomTargetPatch
        {
            private static void Postfix(StaticTarget __instance, ref bool __result)
            {
                if (__result && IsProtected(__instance))
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(BaseAI), "CanSeeTarget", new Type[] { typeof(StaticTarget) })]
        private static class VisibilityPatch
        {
            private static void Postfix(StaticTarget target, ref bool __result)
            {
                if (__result && IsProtected(target))
                {
                    __result = false;
                }
            }
        }
    }
}
