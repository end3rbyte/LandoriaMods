namespace Landoria.Socialize
{
    internal enum PersistentChatChannel { Normal, Shout, Whisper, Group }

    internal static class ChatBehaviorPolicy
    {
        internal static string GetPrompt(PersistentChatChannel channel, string whisperTarget)
        {
            switch (channel)
            {
                case PersistentChatChannel.Shout: return "Shouting...";
                case PersistentChatChannel.Whisper: return "Talking to " + whisperTarget + "...";
                case PersistentChatChannel.Group: return "Speaking to the group...";
                default: return "Speaking...";
            }
        }

        internal static bool ShouldRedirect(
            PersistentChatChannel channel, bool redirecting, string text)
        {
            return !redirecting && channel != PersistentChatChannel.Normal &&
                   !string.IsNullOrWhiteSpace(text);
        }

    }

    internal static class PrivateChatPolicy
    {
        internal static GroupDecision CanSend(bool targetFound, bool isLocalPlayer, string targetName)
        {
            if (!targetFound)
            {
                return GroupDecision.Deny(
                    "No connected player named \"" + targetName + "\" was found.");
            }
            return isLocalPlayer
                ? GroupDecision.Deny("You cannot whisper yourself.")
                : GroupDecision.Allow();
        }
    }

    internal static class TargetPingPolicy
    {
        internal static GroupDecision CanSend(bool targetFound, bool clientReady, string targetName)
        {
            return targetFound && clientReady
                ? GroupDecision.Allow()
                : GroupDecision.Deny(
                    "No connected player named \"" + targetName + "\" was found.");
        }
    }
}
