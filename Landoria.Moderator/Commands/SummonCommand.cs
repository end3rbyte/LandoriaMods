namespace Landoria.Moderator
{
    internal static class SummonCommand
    {
        internal static void Register()
        {
            new Terminal.ConsoleCommand("summon", "[player] - Teleports a player to you.",
                ModeratorCommandAudit.Wrap(Execute),
                isCheat: false, isNetwork: false, onlyServer: false,
                allowInDevBuild: true, onlyAdmin: true);
        }

        private static void Execute(Terminal.ConsoleEventArgs args)
        {
            if (Player.m_localPlayer == null || args.Length < 2 ||
                !PlayerCommandUtility.TryFindPlayer(args[1], out _, out ZDOID characterId))
            {
                ModeratorPlugin.ModLogger.LogError("Summon command failed because the player was not found.");
                args.Context.AddString("Usage: summon <player>");
                return;
            }

            PlayerCommandUtility.Teleport(characterId, Player.m_localPlayer.transform.position,
                Player.m_localPlayer.transform.rotation);
            args.Context.AddString($"Summoned player: {args[1]}");
            ModeratorPlugin.ModLogger.LogDebug(
                $"Summon command teleported '{args[1]}' to the local player.");
        }
    }
}
