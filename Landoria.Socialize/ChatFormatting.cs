using System;
using Splatform;

namespace Landoria.Socialize
{
    internal static class ChatFormatting
    {
        private const string GroupColor = "#4A90E2";
        private const string PrivateColor = "#2FAE5F";
        private const string ShoutColor = "#FFFF00";

        internal static string FormatGroup(string sender, string message)
        {
            return "<color=" + GroupColor + ">" + sender + ": " + message + "</color>";
        }

        internal static void AddPrivate(Terminal terminal, string user, string text, bool timestamp)
        {
            terminal.AddString(GetTimestamp(timestamp) + "<color=" + PrivateColor + ">" +
                               user + FormatPrivateText(text) + "</color>");
        }

        internal static void AddShout(Terminal terminal, string user, string text, bool timestamp)
        {
            terminal.AddString(GetTimestamp(timestamp) + "<color=orange>" + user +
                               "</color>: <color=" + ShoutColor + ">" + text + "</color>");
        }

        internal static void AddPing(Terminal terminal, string user, string target, string message)
        {
            string recipient = string.IsNullOrEmpty(target) ? ": " : " to " + target + ": ";
            terminal.AddString("<color=" + PrivateColor + ">" + user + recipient +
                               "</color><color=" + ShoutColor + ">((Ping))</color>" +
                               "<color=" + PrivateColor + "> " + message + "</color>");
        }

        internal static string GetPlayerName(PlatformUserID user)
        {
            return ZNet.TryGetPlayerByPlatformUserID(user, out ZNet.PlayerInfo info)
                ? info.m_name
                : user.ToString();
        }

        private static string FormatPrivateText(string text)
        {
            return (text ?? "").StartsWith("to ", StringComparison.OrdinalIgnoreCase)
                ? " " + text
                : ": " + text;
        }

        private static string GetTimestamp(bool enabled)
        {
            return enabled ? "[" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "] " : "";
        }
    }
}
