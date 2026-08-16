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
            FirstPersonVisibilityController.SetHidden(player, shouldApply);
            if (shouldApply)
            {
                FirstPersonViewController.Apply(__instance, player);
            }
            else
            {
                FirstPersonHeadLookPatch.ResetTarget();
            }
        }
    }

    [HarmonyPatch(typeof(CharacterAnimEvent), "OnAnimatorIK")]
    internal static class FirstPersonHeadLookPatch
    {
        private const float LookAtSmoothingRate = 18f;
        private static Vector3 smoothedLookAt;
        private static bool lookAtInitialized;

        internal static void ResetTarget()
        {
            lookAtInitialized = false;
            smoothedLookAt = Vector3.zero;
        }

        private static void Postfix(CharacterAnimEvent __instance)
        {
            Player player = __instance.GetComponentInParent<Player>();
            GameCamera camera = GameCamera.instance;
            if (!FirstPersonMode.Active || player != Player.m_localPlayer || !camera)
            {
                return;
            }

            Animator animator = __instance.GetComponent<Animator>();
            Vector3 desiredLookAt = player.GetEyePoint() + camera.transform.forward * 10f;
            float smoothing = 1f - Mathf.Exp(-LookAtSmoothingRate * Time.deltaTime);
            if (!lookAtInitialized)
            {
                smoothedLookAt = desiredLookAt;
                lookAtInitialized = true;
            }
            else
            {
                smoothedLookAt = Vector3.Lerp(
                    smoothedLookAt,
                    desiredLookAt,
                    smoothing);
            }

            animator.SetLookAtPosition(smoothedLookAt);
            animator.SetLookAtWeight(0.8f, 0f, 0.8f, 0f, 0f);
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

    [HarmonyPatch(typeof(VisEquipment), "UpdateEquipmentVisuals")]
    internal static class FirstPersonEquipmentVisibilityPatch
    {
        private static void Postfix(VisEquipment __instance)
        {
            Player player = __instance.GetComponentInParent<Player>();
            if (FirstPersonMode.Active && player == Player.m_localPlayer)
            {
                FirstPersonVisibilityController.Refresh(player);
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
            FirstPersonVisibilityController.Restore();
            FirstPersonMode.ResetSession();
        }
    }
}
