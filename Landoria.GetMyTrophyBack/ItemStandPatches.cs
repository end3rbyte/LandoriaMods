using HarmonyLib;

namespace Landoria.GetMyTrophyBack
{
    [HarmonyPatch(typeof(ItemStand), "DelayedPowerActivation")]
    internal static class DropTrophyAfterPowerActivationPatch
    {
        private static void Postfix(ItemStand __instance)
        {
            if (!GetMyTrophyBackPlugin.IsEnabled)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null || __instance.m_guardianPower == null)
            {
                return;
            }

            if (player.GetGuardianPowerName() != __instance.m_guardianPower.name)
            {
                return;
            }

            __instance.StartCoroutine(TrophyDropService.DropAfterDelay(__instance));
            GetMyTrophyBackPlugin.Log.LogDebug($"Scheduled trophy drop for {__instance.m_guardianPower.name}.");
        }
    }

    [HarmonyPatch(typeof(ItemStand), "Awake")]
    internal static class RegisterTrophyDropRpcPatch
    {
        private static void Postfix(ItemStand __instance, ZNetView ___m_nview)
        {
            if (__instance.m_guardianPower == null || ___m_nview == null ||
                ___m_nview.GetZDO() == null)
            {
                return;
            }

            ___m_nview.Register(
                TrophyDropService.RpcName,
                sender => TrophyDropService.HandleDropRequest(__instance, ___m_nview, sender));
            GetMyTrophyBackPlugin.Log.LogDebug($"Registered trophy drop RPC for {__instance.m_guardianPower.name}.");
        }
    }
}
