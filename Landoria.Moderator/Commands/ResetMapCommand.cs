namespace Landoria.Moderator
{
    internal static class ResetMapCommand
    {
        internal static void Register()
        {
            new Terminal.ConsoleCommand("resetmap", "Hides the entire explored map.",
                ModeratorCommandAudit.Wrap(Execute),
                isCheat: false, isNetwork: false, onlyServer: false,
                allowInDevBuild: true, onlyAdmin: true);
        }

        private static void Execute(Terminal.ConsoleEventArgs args)
        {
            if (Minimap.instance == null)
            {
                ModeratorPlugin.ModLogger.LogError("Resetmap command failed because the map is unavailable.");
                args.Context.AddString("The map is not available yet.");
                return;
            }

            Minimap.instance.Reset();
            ModeratorMapSharing.Disable();
            args.Context.AddString("The entire map exploration has been reset.");
            ModeratorPlugin.ModLogger.LogDebug("Resetmap command cleared the local map exploration.");
        }
    }
}
