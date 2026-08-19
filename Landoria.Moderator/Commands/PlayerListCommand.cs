using System.Collections.Generic;
using Landoria.SharedLib;

namespace Landoria.Moderator
{
    internal static class PlayerListCommand
    {
        internal static void Register()
        {
            new Terminal.ConsoleCommand("playerlist", "Prints online players.",
                ModeratorCommandAudit.Wrap(Execute),
                isCheat: false, isNetwork: false, onlyServer: false,
                allowInDevBuild: true, onlyAdmin: true);
        }

        private static void Execute(Terminal.ConsoleEventArgs args)
        {
            if (ZNet.instance == null)
            {
                ModeratorPlugin.ModLogger.LogError("Playerlist command failed because ZNet is unavailable.");
                args.Context.AddString("The player list is not available.");
                return;
            }

            List<string> players = new List<string>();
            foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
            {
                string moderatorSuffix = IsAdmin(player)
                    ? "  " + ModeratorState.ModeratorLabel : string.Empty;
                players.Add(player.m_name + moderatorSuffix);
            }

            args.Context.AddString(string.Join("\n", players));
            ModeratorPlugin.ModLogger.LogDebug(
                $"Playerlist command returned {players.Count} online player entries.");
        }

        private static bool IsAdmin(ZNet.PlayerInfo player)
        {
            string platformPlayerId = PlatformPlayerIdentity.Resolve(player);
            if (string.IsNullOrWhiteSpace(platformPlayerId))
            {
                return false;
            }
            if (ZNet.instance.IsAdmin(platformPlayerId))
            {
                return true;
            }

            return ZNet.instance.IsServer() &&
                   platformPlayerId == UserInfo.GetLocalUser().UserId.ToString();
        }
    }
}
