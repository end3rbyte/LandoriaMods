using HarmonyLib;

namespace Landoria.HammerFreedom
{
    [HarmonyPatch(typeof(Terminal), "InitTerminal")]
    internal static class FlyCommandRegistrationPatch
    {
        private static void Postfix()
        {
            FlyCommand.Register();
        }
    }

    [HarmonyPatch(typeof(Terminal.ConsoleCommand), "IsValid")]
    internal static class FlyCommandValidationPatch
    {
        private static void Postfix(Terminal.ConsoleCommand __instance, ref bool __result)
        {
            if (FlyCommand.IsCommand(__instance) && !HammerFreedomAuthorization.IsAuthorized(
                    HammerFreedomCapabilities.Flight))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Character), "Damage")]
    internal static class FallDamagePatch
    {
        private static bool Prefix(Character __instance, HitData hit)
        {
            return HammerFreedomBehaviorPolicy.ShouldApplyDamage(
                __instance == Player.m_localPlayer,
                hit.m_hitType == HitData.HitType.Fall,
                HammerFreedomAuthorization.IsAuthorized(
                    HammerFreedomCapabilities.FallDamageImmunity));
        }
    }

    [HarmonyPatch(typeof(Player), "UseStamina")]
    internal static class StaminaConsumptionPatch
    {
        private static bool Prefix(Player __instance)
        {
            return HammerFreedomBehaviorPolicy.ShouldConsumeStamina(
                __instance == Player.m_localPlayer,
                HammerFreedomAuthorization.IsAuthorized(
                    HammerFreedomCapabilities.UnlimitedStamina));
        }
    }

    [HarmonyPatch(typeof(Player), "RPC_UseStamina")]
    internal static class StaminaApplicationPatch
    {
        private static bool Prefix(Player __instance)
        {
            return HammerFreedomBehaviorPolicy.ShouldConsumeStamina(
                __instance == Player.m_localPlayer,
                HammerFreedomAuthorization.IsAuthorized(
                    HammerFreedomCapabilities.UnlimitedStamina));
        }
    }

    [HarmonyPatch(typeof(ZNet), "OnDestroy")]
    internal static class HammerFreedomDisconnectPatch
    {
        private static void Prefix()
        {
            HammerFreedomAuthorization.ResetSession();
        }
    }
}
