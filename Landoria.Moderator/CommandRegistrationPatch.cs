using HarmonyLib;

namespace Landoria.Moderator
{
    [HarmonyPatch(typeof(Terminal), "InitTerminal")]
    internal static class CommandRegistrationPatch
    {
        private static void Postfix()
        {
            ModeratorPlugin.RegisterCommands();
            ModeratorPlugin.ModLogger.LogDebug(
                "Custom commands were registered after vanilla terminal initialization.");
        }
    }
}
