namespace Landoria.Socialize
{
    internal static class GroupCommands
    {
        internal static void Register()
        {
            new Terminal.ConsoleCommand(
                "group",
                "[help|invite|leave|remove|promote|info] manages your group",
                HandleGroup);
            new Terminal.ConsoleCommand(
                "g", "[message] sends a message to your group.", HandleGroupChat);
        }

        private static void HandleGroup(Terminal.ConsoleEventArgs args)
        {
            string text = (args.ArgsAll ?? "").Trim();
            int separator = text.IndexOf(' ');
            string action = separator < 0 ? text.ToLowerInvariant() : text.Substring(0, separator).ToLowerInvariant();
            string argument = separator < 0 ? "" : text.Substring(separator + 1).Trim();
            if (action == "help" || string.IsNullOrEmpty(action))
            {
                ShowHelp(args.Context);
                return;
            }
            if (!ChatCommandParser.IsValidGroupAction(action, argument))
            {
                ShowHelp(args.Context);
                return;
            }
            GroupService.SendRequest(action, argument);
        }

        private static void HandleGroupChat(Terminal.ConsoleEventArgs args)
        {
            string message = (args.ArgsAll ?? "").Trim();
            if (string.IsNullOrEmpty(message))
            {
                args.Context.AddString("Usage: /g message");
                return;
            }
            ChatChannelState.SetGroup();
            GroupService.SendChat(message);
        }

        private static void ShowHelp(Terminal context)
        {
            context.AddString("/group invite <PlayerName> - Invites a connected player.");
            context.AddString("/group leave - Leaves your group.");
            context.AddString("/group remove <PlayerName> - Removes a member.");
            context.AddString("/group promote <PlayerName> - Promotes a member.");
            context.AddString("/group info - Lists group members.");
            context.AddString("/g <message> - Sends a group message.");
        }
    }
}
