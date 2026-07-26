namespace Landoria.Moderator
{
    internal static class ExploreMapCommand
    {
        internal static void Register()
        {
            new Terminal.ConsoleCommand(
                "exploremap", "Explores the entire map.", ModeratorCommandAudit.Wrap(Execute),
                isCheat: false, isNetwork: false, onlyServer: false,
                allowInDevBuild: true, onlyAdmin: true);
        }

        private static void Execute(Terminal.ConsoleEventArgs args)
        {
            if (Minimap.instance == null)
            {
                ModeratorPlugin.ModLogger.LogError("Exploremap command failed because the map is not available.");
                args.Context.AddString("The map is not available yet.");
                return;
            }

            Minimap.instance.ExploreAll();
            ModeratorMapSharing.Enable();
            args.Context.AddString("The entire map has been explored and connected players are visible.");
            ModeratorPlugin.ModLogger.LogDebug(
                "Exploremap command revealed the local map and enabled player tracking.");
        }
    }
}
