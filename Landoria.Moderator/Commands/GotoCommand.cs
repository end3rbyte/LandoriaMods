using UnityEngine;

namespace Landoria.Moderator
{
    internal static class GotoCommand
    {
        internal static void Register()
        {
            new Terminal.ConsoleCommand("goto", "[player] - Teleports you to a player.",
                ModeratorCommandAudit.Wrap(Execute),
                isCheat: false, isNetwork: false, onlyServer: false,
                allowInDevBuild: true, onlyAdmin: true);
        }

        private static void Execute(Terminal.ConsoleEventArgs args)
        {
            if (Player.m_localPlayer == null || args.Length != 2)
            {
                Fail(args, "Usage: goto <player>");
                return;
            }

            string playerName = args[1];
            if (!PlayerPositionRpc.Request(
                    playerName, position => Teleport(position, args.Context)))
            {
                Fail(args, "No matching player was found.");
            }
        }

        private static void Teleport(Vector3 destination, Terminal context)
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }

            Player.m_localPlayer.TeleportTo(destination,
                Player.m_localPlayer.transform.rotation, distantTeleport: true);
            context.AddString($"Teleported to: {destination}");
            ModeratorPlugin.ModLogger.LogDebug(
                $"Goto command teleported the local player to {destination}.");
        }

        private static void Fail(Terminal.ConsoleEventArgs args, string message)
        {
            ModeratorPlugin.ModLogger.LogError($"Goto command failed: {message}");
            args.Context.AddString(message);
        }
    }
}
