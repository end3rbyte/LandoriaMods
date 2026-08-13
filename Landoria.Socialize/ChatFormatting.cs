using System;
using Splatform;

namespace Landoria.Socialize
{
    internal static class ChatFormatting
    {
        internal static string FormatGroup(string sender, string message)
        {
            return ChatFormattingPolicy.FormatGroup(sender, message);
        }

        internal static void AddPrivate(Terminal terminal, string user, string text, bool timestamp)
        {
            terminal.AddString(GetTimestamp(timestamp) +
                               ChatFormattingPolicy.FormatPrivate(user, text));
        }

        internal static void AddShout(Terminal terminal, string user, string text, bool timestamp)
        {
            terminal.AddString(GetTimestamp(timestamp) +
                               ChatFormattingPolicy.FormatShout(user, text));
        }

        internal static void AddPing(Terminal terminal, string user, string target, string message)
        {
            terminal.AddString(ChatFormattingPolicy.FormatPing(user, target, message));
        }

        internal static string GetPlayerName(PlatformUserID user)
        {
            return ZNet.TryGetPlayerByPlatformUserID(user, out ZNet.PlayerInfo info)
                ? info.m_name
                : user.ToString();
        }

        private static string GetTimestamp(bool enabled)
        {
            return enabled ? "[" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "] " : "";
        }
    }
}
