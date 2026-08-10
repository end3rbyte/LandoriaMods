using HarmonyLib;

namespace Landoria.FlyCommand
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
            if (FlyCommand.IsCommand(__instance) && !FlyAuthorization.IsAuthorized)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), "OnDestroy")]
    internal static class FlyDisconnectPatch
    {
        private static void Prefix()
        {
            FlyAuthorization.ResetSession();
        }
    }
}
