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
        private static void Postfix(GameCamera __instance, float ___m_distance)
        {
            Player player = Player.m_localPlayer;
            bool shouldApply = FirstPersonPolicy.ShouldApplyFirstPerson(
                FirstPersonMode.Enabled, player, player && player.IsDead(),
                GameCamera.InFreeFly(), ___m_distance);
            if (shouldApply)
            {
                FirstPersonViewController.Apply(__instance, player);
            }
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

    [HarmonyPatch(typeof(Terminal.ConsoleCommand), "RunAction")]
    internal static class FirstPersonFieldOfViewCommandPatch
    {
        private static bool Prefix(
            Terminal.ConsoleCommand __instance, Terminal.ConsoleEventArgs args)
        {
            string value = args.Length > 1 ? args[1] : null;
            if (!FirstPersonPolicy.ShouldResetFieldOfView(
                    __instance.Command, args.Length, value))
            {
                return true;
            }

            float fieldOfView = FirstPersonPreference.DefaultFieldOfView;
            FirstPersonMode.SetFieldOfView(GameCamera.instance, fieldOfView);
            FirstPersonPreference.SetFieldOfView(fieldOfView);
            return false;
        }

        private static void Postfix(
            Terminal.ConsoleCommand __instance, Terminal.ConsoleEventArgs args)
        {
            bool parsed = args.TryParameterFloat(1, out float fieldOfView);
            if (FirstPersonPolicy.ShouldPersistFieldOfView(
                __instance.Command, args.Length, parsed, fieldOfView))
            {
                fieldOfView = FirstPersonPolicy.ClampFieldOfView(fieldOfView);
                FirstPersonMode.SetFieldOfView(GameCamera.instance, fieldOfView);
                FirstPersonPreference.SetFieldOfView(fieldOfView);
            }
        }
    }

    [HarmonyPatch(typeof(Player), "OnSpawned")]
    internal static class FirstPersonPlayerSpawnPatch
    {
        private static void Postfix(Player __instance)
        {
            if (__instance == Player.m_localPlayer)
            {
                FirstPersonMode.SetEnabled(FirstPersonPreference.Enabled);
                FirstPersonMode.SetFieldOfView(
                    GameCamera.instance, FirstPersonPreference.FieldOfView);
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
