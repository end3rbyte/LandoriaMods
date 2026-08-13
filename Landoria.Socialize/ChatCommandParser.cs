using System;

namespace Landoria.Socialize
{
    internal static class ChatCommandParser
    {
        internal static bool TryParseTarget(
            string fullLine, out string target, out string message)
        {
            target = "";
            message = "";
            if (string.IsNullOrEmpty(fullLine))
            {
                return false;
            }
            int targetStart = fullLine.IndexOf(' ');
            if (targetStart < 0)
            {
                return false;
            }
            int messageStart = fullLine.IndexOf(' ', targetStart + 1);
            string token = messageStart >= 0
                ? fullLine.Substring(targetStart + 1, messageStart - targetStart - 1)
                : fullLine.Substring(targetStart + 1);
            target = token.Trim();
            if (target.StartsWith("@", StringComparison.Ordinal))
            {
                target = target.Substring(1);
            }
            message = messageStart >= 0 ? fullLine.Substring(messageStart + 1) : "";
            return !string.IsNullOrWhiteSpace(target) && !string.IsNullOrWhiteSpace(message);
        }

        internal static bool IsValidGroupAction(string action, string argument)
        {
            switch (action)
            {
                case "leave":
                case "info": return string.IsNullOrEmpty(argument);
                case "invite":
                case "remove":
                case "promote": return !string.IsNullOrEmpty(argument);
                default: return false;
            }
        }
    }
}
