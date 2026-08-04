using HarmonyLib;

namespace Landoria.GentleDeath
{
    [HarmonyPatch(typeof(Player), "CreateTombStone")]
    internal static class CreateTombstonePatch
    {
        private static bool Prefix(Player __instance)
        {
            if (!GentleDeathPlugin.IsEnabled)
            {
                return true;
            }

            DeathInventory.CreateTombstone(__instance);
            return false;
        }
    }
}
