using HarmonyLib;
using UnityEngine;

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
            FirstPersonMode.SetActive(shouldApply);
            FirstPersonMode.ApplyConfiguredFieldOfView(__instance);
            FirstPersonVisibilityController.SetHidden(player, shouldApply);
            if (shouldApply)
            {
                FirstPersonViewController.Apply(__instance, player);
                FirstPersonHelmetLightController.Apply(__instance, player);
            }
            else
            {
                FirstPersonHelmetLightController.Restore();
            }
        }
    }

    [HarmonyPatch(typeof(Character), "SetVisible")]
    internal static class FirstPersonPlayerVisibilityPatch
    {
        private static void Prefix(Character __instance, ref bool visible)
        {
            if (FirstPersonMode.Active && __instance == Player.m_localPlayer)
            {
                visible = true;
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), "UpdateVisuals")]
    internal static class FirstPersonVisualVisibilityPatch
    {
        private static void Postfix(
            VisEquipment __instance, GameObject ___m_leftItemInstance,
            GameObject ___m_rightItemInstance)
        {
            Player player = __instance.GetComponentInParent<Player>();
            if (player == Player.m_localPlayer)
            {
                FirstPersonVisibilityController.TrackHeldItems(
                    player, ___m_leftItemInstance, ___m_rightItemInstance);
            }
            if (FirstPersonMode.Active && player == Player.m_localPlayer)
            {
                FirstPersonHelmetLightController.Refresh(player);
            }
        }
    }

    [HarmonyPatch(typeof(MonoUpdaters), "LateUpdate")]
    internal static class FirstPersonHelmetLightLateUpdatePatch
    {
        private static void Postfix()
        {
            FirstPersonHelmetLightController.Apply(
                GameCamera.instance, Player.m_localPlayer);
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
                bool parsed = args.TryParameterFloat(1, out float requestedFieldOfView);
                if (!FirstPersonPolicy.ShouldRejectFieldOfView(
                        __instance.Command, args.Length, parsed, requestedFieldOfView))
                {
                    return true;
                }

                args.Context?.AddString(FirstPersonMessages.FieldOfViewAboveMaximum);
                return false;
            }

            float fieldOfView = FirstPersonPreference.DefaultFieldOfView;
            FirstPersonPreference.SetFieldOfView(fieldOfView);
            FirstPersonMode.ApplyConfiguredFieldOfView(GameCamera.instance);
            return false;
        }

        private static void Postfix(
            Terminal.ConsoleCommand __instance, Terminal.ConsoleEventArgs args)
        {
            bool parsed = args.TryParameterFloat(1, out float fieldOfView);
            if (FirstPersonPolicy.ShouldPersistFieldOfView(
                __instance.Command, args.Length, parsed, fieldOfView))
            {
                FirstPersonPreference.SetFieldOfView(fieldOfView);
                FirstPersonMode.ApplyConfiguredFieldOfView(GameCamera.instance);
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
                FirstPersonMode.ApplyConfiguredFieldOfView(GameCamera.instance);
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), "OnDestroy")]
    internal static class FirstPersonDisconnectPatch
    {
        private static void Prefix()
        {
            FirstPersonHelmetLightController.Restore();
            FirstPersonVisibilityController.Restore();
            FirstPersonMode.ResetSession();
        }
    }
}
