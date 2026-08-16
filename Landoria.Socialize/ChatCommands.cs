using System;

namespace Landoria.Socialize
{
    internal static class ChatCommands
    {
        internal static void Register()
        {
            Register("sh", SendShout);
            Register("shout", SendShout);
            Register("s", SendSay);
            Register("say", SendSay);
            Register("w", SendWhisper);
            Register("wping", SendTargetPing);
        }

        private static void Register(string name, Terminal.ConsoleEventFailable handler)
        {
            new Terminal.ConsoleCommand(name, GetDescription(name), handler);
        }

        private static string GetDescription(string name)
        {
            if (name == "w")
            {
                return "[player] [message] sends a private message";
            }
            if (name == "wping")
            {
                return "[player] [message] sends a private message with a ping";
            }
            return "[message] " + (name == "s" || name == "say"
                ? "says something to nearby players"
                : "shouts so everyone around you can hear you");
        }

        private static object SendShout(Terminal.ConsoleEventArgs args)
        {
            if (!TryGetMessage(args, out string message))
            {
                args.Context.AddString("Usage: /sh message");
                return true;
            }
            ChatChannelState.SetShout();
            SocialChatSender.SendShout(message);
            return true;
        }

        private static object SendSay(Terminal.ConsoleEventArgs args)
        {
            if (!TryGetMessage(args, out string message))
            {
                args.Context.AddString("Usage: /s message");
                return true;
            }
            ChatChannelState.SetNormal();
            Chat.instance.SendText(Talker.Type.Normal, message);
            return true;
        }

        private static object SendWhisper(Terminal.ConsoleEventArgs args)
        {
            if (!TryParseTarget(args.FullLine, out string target, out string message))
            {
                args.Context.AddString("Usage: /w PlayerName message");
                return true;
            }
            if (!PrivateChat.Send(target, message, args.Context))
            {
                return true;
            }
            ChatChannelState.SetWhisper(target);
            return true;
        }

        private static object SendTargetPing(Terminal.ConsoleEventArgs args)
        {
            if (!TryParseTarget(args.FullLine, out string target, out string message))
            {
                args.Context.AddString("Usage: /wping PlayerName message");
                return true;
            }
            TargetPingService.Send(target, message, args.Context);
            return true;
        }

        private static bool TryGetMessage(Terminal.ConsoleEventArgs args, out string message)
        {
            message = (args.ArgsAll ?? "").Trim();
            return Chat.instance != null && !string.IsNullOrEmpty(message);
        }

        private static bool TryParseTarget(string fullLine, out string target, out string message)
        {
            target = "";
            message = "";
            int targetStart = fullLine.IndexOf(' ');
            if (targetStart < 0) return false;
            int messageStart = fullLine.IndexOf(' ', targetStart + 1);
            string token = messageStart >= 0
                ? fullLine.Substring(targetStart + 1, messageStart - targetStart - 1)
                : fullLine.Substring(targetStart + 1);
            target = token.Trim();
            if (target.StartsWith("@", StringComparison.Ordinal)) target = target.Substring(1);
            message = messageStart >= 0 ? fullLine.Substring(messageStart + 1) : "";
            return !string.IsNullOrWhiteSpace(target) && !string.IsNullOrWhiteSpace(message);
        }
    }

    internal static class PrivateChat
    {
        internal static bool Send(string targetName, string message, Terminal context)
        {
            if (!TryFindPlayer(targetName, out ZNet.PlayerInfo target))
            {
                context?.AddString("No connected player named \"" + targetName + "\" was found.");
                return false;
            }
            if (IsLocalPlayer(target))
            {
                context?.AddString("You cannot whisper yourself.");
                return false;
            }
            SendToTarget(target, message);
            string localName = Game.instance.GetPlayerProfile().GetName();
            ChatFormatting.AddPrivate(context, localName, "to " + target.m_name + ": " + message, false);
            return true;
        }

        private static void SendToTarget(ZNet.PlayerInfo target, string message)
        {
            Chat.GetChatMessageData(message, true, out UserInfo user, out string filteredMessage);
            ZRoutedRpc.instance.InvokeRoutedRPC(
                target.m_characterID.UserID,
                "ChatMessage",
                Player.m_localPlayer.GetHeadPoint(),
                (int)Talker.Type.Whisper,
                user,
                filteredMessage);
        }

        private static bool TryFindPlayer(string name, out ZNet.PlayerInfo player)
        {
            foreach (ZNet.PlayerInfo candidate in ZNet.instance.GetPlayerList())
            {
                if (string.Equals(candidate.m_name, name, StringComparison.OrdinalIgnoreCase))
                {
                    player = candidate;
                    return true;
                }
            }
            player = default;
            return false;
        }

        private static bool IsLocalPlayer(ZNet.PlayerInfo player)
        {
            return ZNet.instance != null
                   && player.m_characterID.UserID
                   == ZNet.instance.LocalPlayerCharacterID.UserID;
        }
    }
}
