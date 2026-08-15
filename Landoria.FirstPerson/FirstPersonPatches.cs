using HarmonyLib;

namespace Landoria.FirstPerson
{
    [HarmonyPatch(typeof(GameCamera), "Awake")]
    internal static class FirstPersonCameraAwakePatch
    {
        private static void Prefix(GameCamera __instance)
        {
            FirstPersonMode.CaptureVanillaDistance(__instance);
        }

        private static void Postfix(GameCamera __instance)
        {
            FirstPersonMode.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
    internal static class FirstPersonCameraUpdatePatch
    {
        private static void Postfix(float ___m_distance)
        {
            Player player = Player.m_localPlayer;
            bool shouldHide = FirstPersonPolicy.ShouldHidePlayer(
                FirstPersonMode.Enabled, player, player && player.IsDead(),
                GameCamera.InFreeFly(), ___m_distance);
            LocalPlayerVisibility.Update(player, shouldHide);
        }
    }

    [HarmonyPatch(typeof(Terminal), "InitTerminal")]
    internal static class FirstPersonCommandRegistrationPatch
    {
        private static void Postfix()
        {
            FirstPersonCommand.Register();
        }
    }

    [HarmonyPatch(typeof(Player), "OnSpawned")]
    internal static class FirstPersonPlayerSpawnPatch
    {
        private static void Postfix(Player __instance)
        {
            if (__instance == Player.m_localPlayer)
            {
                FirstPersonMode.SetEnabled(FirstPersonPreference.Load(__instance));
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), "UpdateLodgroup")]
    internal static class FirstPersonEquipmentUpdatePatch
    {
        private static void Postfix(VisEquipment __instance)
        {
            if (__instance.GetComponentInParent<Player>() == Player.m_localPlayer)
            {
                LocalPlayerVisibility.Refresh();
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), "OnDestroy")]
    internal static class FirstPersonDisconnectPatch
    {
        private static void Prefix()
        {
            FirstPersonMode.ResetSession();
        }
    }
}
