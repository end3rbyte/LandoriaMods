namespace Landoria.Moderator
{
    internal static class ModeratorModeCommand
    {
        internal static void Register()
        {
            new Terminal.ConsoleCommand(
                "moderator", "Toggles moderator mode.", ModeratorCommandAudit.Wrap(Execute),
                isCheat: false, isNetwork: false, onlyServer: false,
                allowInDevBuild: true, onlyAdmin: true);
        }

        private static void Execute(Terminal.ConsoleEventArgs args)
        {
            bool enabled = !ModeratorState.IsActive;
            ModeratorState.SetEnabled(enabled);
            string state = enabled ? "enabled" : "disabled";
            args.Context?.AddString($"Moderator mode {state}.");
            ModeratorPlugin.ModLogger.LogInfo($"Moderator mode {state}.");
        }
    }
}
