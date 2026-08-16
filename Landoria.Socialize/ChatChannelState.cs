using System;

namespace Landoria.Socialize
{
    internal static class ChatChannelState
    {
        private enum Channel { Normal, Shout, Whisper, Group }
        private static Channel current;
        private static string whisperTarget = "";
        private static bool redirecting;

        internal static void SetNormal()
        {
            current = Channel.Normal;
            whisperTarget = "";
        }

        internal static void SetShout()
        {
            current = Channel.Shout;
            whisperTarget = "";
        }

        internal static void SetWhisper(string target)
        {
            current = Channel.Whisper;
            whisperTarget = target ?? "";
        }

        internal static void SetGroup()
        {
            current = Channel.Group;
            whisperTarget = "";
        }

        internal static string GetPrompt()
        {
            switch (current)
            {
                case Channel.Shout: return "Shouting...";
                case Channel.Whisper: return "Talking to " + whisperTarget + "...";
                case Channel.Group: return "Speaking to the group...";
                default: return "Speaking...";
            }
        }

        internal static bool TryRedirect(string text)
        {
            if (redirecting || current == Channel.Normal || string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            redirecting = true;
            try
            {
                return Send(text);
            }
            finally
            {
                redirecting = false;
            }
        }

        private static bool Send(string text)
        {
            switch (current)
            {
                case Channel.Shout: SocialChatSender.SendShout(text); return true;
                case Channel.Whisper: return PrivateChat.Send(whisperTarget, text, Chat.instance);
                case Channel.Group: GroupService.SendChat(text); return true;
                default: return false;
            }
        }
    }

    internal static class SocialChatSender
    {
        internal static void SendShout(string text)
        {
            Talker talker = Player.m_localPlayer != null
                ? Player.m_localPlayer.GetComponent<Talker>()
                : null;
            if (talker == null)
            {
                return;
            }
            talker.Say(Talker.Type.Shout, text);
        }

        internal static void ApplyShoutRange(Talker talker)
        {
            talker.m_shoutDistance = talker.m_normalDistance * 2f;
        }
    }
}
