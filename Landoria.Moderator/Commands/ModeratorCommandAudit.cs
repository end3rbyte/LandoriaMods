namespace Landoria.Moderator
{
    internal static class ModeratorCommandAudit
    {
        internal static Terminal.ConsoleEvent Wrap(Terminal.ConsoleEvent execute)
        {
            return args =>
            {
                string playerName = Player.m_localPlayer?.GetPlayerName() ?? "unknown";
                ModeratorPlugin.ModLogger.LogInfo(
                    $"Moderator command invoked by '{playerName}': {args.FullLine}");
                execute(args);
            };
        }
    }
}
