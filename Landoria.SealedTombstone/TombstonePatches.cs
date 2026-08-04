using HarmonyLib;

namespace Landoria.SealedTombstone
{
    [HarmonyPatch(typeof(Game), "Start")]
    internal static class RpcRegistrationPatch
    {
        private static void Postfix()
        {
            TombstoneAccess.RegisterRpcs();
        }
    }

    [HarmonyPatch(typeof(TombStone), "Setup")]
    internal static class TombstoneSetupPatch
    {
        private static void Postfix(TombStone __instance, long ownerUID)
        {
            if (SealedTombstonePlugin.IsEnabled)
            {
                TombstoneAccess.RecordLockDay(__instance, ownerUID);
            }
        }
    }

    [HarmonyPatch(typeof(TombStone), "Interact")]
    internal static class TombstoneInteractPatch
    {
        private static bool Prefix(TombStone __instance, Humanoid character)
        {
            return !SealedTombstonePlugin.IsEnabled ||
                   TombstoneAccess.AllowInteraction(__instance, character);
        }
    }

    [HarmonyPatch(typeof(Player), "OnDamaged")]
    internal static class PlayerDamagedPatch
    {
        private static void Postfix(Player __instance, HitData hit)
        {
            if (SealedTombstonePlugin.IsEnabled)
            {
                RecentAttackers.Record(__instance, hit);
            }
        }
    }

    [HarmonyPatch(typeof(Player), "OnDeath")]
    internal static class PlayerDeathPatch
    {
        private static void Prefix(Player __instance)
        {
            if (SealedTombstonePlugin.IsEnabled)
            {
                RecentAttackers.SnapshotForDeath(__instance);
            }
        }
    }
}
