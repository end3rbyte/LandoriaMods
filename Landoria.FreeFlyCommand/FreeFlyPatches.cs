using HarmonyLib;

namespace Landoria.FreeFlyCommand
{
    [HarmonyPatch(typeof(Player), "OnSpawned")]
    internal static class FreeFlyAuthorizationOnSpawnPatch
    {
        private static void Postfix(Player __instance)
        {
            if (__instance == Player.m_localPlayer)
            {
                FreeFlyAuthorization.RequestOnSpawn();
            }
        }
    }

    [HarmonyPatch(typeof(Terminal), "InitTerminal")]
    internal static class FreeFlyCommandRegistrationPatch
    {
        private static void Postfix()
        {
            FreeFlyCommands.Register();
        }
    }

    [HarmonyPatch(typeof(Terminal.ConsoleCommand), "IsValid")]
    internal static class FreeFlyCommandValidationPatch
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Terminal.ConsoleCommand __instance, ref bool __result)
        {
            if (FreeFlyCommands.IsManaged(__instance) && !FreeFlyAuthorization.IsAuthorized)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(GameCamera), "ToggleFreeFly")]
    internal static class UnauthorizedFreeFlyTogglePatch
    {
        private static bool Prefix()
        {
            return FreeFlyAuthorization.IsAuthorized || GameCamera.InFreeFly();
        }
    }

    [HarmonyPatch(typeof(GameCamera), "UpdateFreeFly")]
    internal static class FreeFlyDistancePatch
    {
        private static void Postfix(GameCamera __instance)
        {
            FreeFlyController.ClampToPlayer(__instance);
        }
    }

    [HarmonyPatch(typeof(ZNet), "OnDestroy")]
    internal static class FreeFlyDisconnectPatch
    {
        private static void Prefix()
        {
            FreeFlyAuthorization.ResetSession();
        }
    }
}
