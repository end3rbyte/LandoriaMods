using HarmonyLib;

using UnityEngine;

namespace Landoria.FirstPerson
{
    internal static class FirstPersonCameraPolicy
    {
        private const float EyeForwardOffset = 0.10f;
        private const float ChinVerticalOffset = 0.10f;

        internal static void ApplyChinLock(GameCamera camera, Player player, float cameraDistance)
        {
            if (!FirstPersonMode.Enabled || camera == null || GameCamera.InFreeFly() ||
                player == null || player.IsDead() || cameraDistance > FirstPersonPolicy.DistanceThreshold)
            {
                return;
            }

            Vector3 lookDirection = camera.transform.forward;
            if (lookDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector3 target = player.GetHeadPoint() - Vector3.up * ChinVerticalOffset +
                             lookDirection.normalized * EyeForwardOffset;
            camera.transform.position = target;
        }
    }

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
            LocalPlayerVisibility.Reset();
            FirstPersonCameraPolicy.ApplyChinLock(__instance, player, ___m_distance);
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
