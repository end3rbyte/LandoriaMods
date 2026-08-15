using System;

namespace Landoria.Socialize
{
    internal static class ChatChannelState
    {
        private static PersistentChatChannel current;
        private static string whisperTarget = "";
        private static bool redirecting;

        internal static void SetNormal()
        {
            current = PersistentChatChannel.Normal;
            whisperTarget = "";
        }

        internal static void SetShout()
        {
            current = PersistentChatChannel.Shout;
            whisperTarget = "";
        }

        internal static void SetWhisper(string target)
        {
            current = PersistentChatChannel.Whisper;
            whisperTarget = target ?? "";
        }

        internal static void SetGroup()
        {
            current = PersistentChatChannel.Group;
            whisperTarget = "";
        }

        internal static string GetPrompt()
        {
            return ChatBehaviorPolicy.GetPrompt(current, whisperTarget);
        }

        internal static bool TryRedirect(string text)
        {
            if (!ChatBehaviorPolicy.ShouldRedirect(current, redirecting, text))
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
                case PersistentChatChannel.Shout: SocialChatSender.SendShout(text); return true;
                case PersistentChatChannel.Whisper: return PrivateChat.Send(whisperTarget, text, Chat.instance);
                case PersistentChatChannel.Group: GroupService.SendChat(text); return true;
                default: return false;
            }
        }
    }

    internal static class SocialChatSender
    {
        private static bool sendingAll;

        internal static bool IsSendingAll => sendingAll;

        internal static void SendAll(string text)
        {
            if (Chat.instance == null)
            {
                return;
            }
            sendingAll = true;
            try
            {
                Chat.instance.SendText(Talker.Type.Shout, text);
            }
            finally
            {
                sendingAll = false;
            }
        }

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

        internal static void ApplyRanges(Talker talker)
        {
            talker.m_normalDistance = SocializePlugin.Settings.SayDistance;
            talker.m_shoutDistance = SocializePlugin.Settings.ShoutDistance;
        }

        internal static void ApplyRangesToLoadedTalkers()
        {
            if (SocializePlugin.Settings == null)
            {
                return;
            }
            Talker[] talkers = UnityEngine.Object.FindObjectsByType<Talker>(
                UnityEngine.FindObjectsSortMode.None);
            foreach (Talker talker in talkers)
            {
                ApplyRanges(talker);
            }
        }
    }
}
