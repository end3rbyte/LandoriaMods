using HarmonyLib;
using Splatform;
using TMPro;
using UnityEngine;

namespace Landoria.Socialize
{
    [HarmonyPatch(typeof(Terminal), "InitTerminal")]
    internal static class RegisterSocialCommandsPatch
    {
        private static bool registered;

        private static void Postfix()
        {
            if (registered)
            {
                return;
            }
            registered = true;
            ChatCommands.Register();
            GroupCommands.Register();
        }
    }

    [HarmonyPatch(typeof(Chat), "AddInworldText")]
    internal static class DisablePrivateWorldTextPatch
    {
        private static bool Prefix(Talker.Type type)
        {
            return type != Talker.Type.Whisper;
        }
    }

    [HarmonyPatch(typeof(Chat), "SendPing")]
    internal static class LimitMapPingToGroupPatch
    {
        private static bool Prefix()
        {
            if (GroupService.IsLocalPlayerInGroup()) return true;
            SocializePlugin.Log.LogDebug(
                "Map ping ignored because the local player is not in a group.");
            return false;
        }
    }

    [HarmonyPatch(typeof(Chat), "SendInput")]
    internal static class PersistentChatInputPatch
    {
        private static bool Prefix(Chat __instance)
        {
            if (__instance.m_input == null || string.IsNullOrWhiteSpace(__instance.m_input.text) ||
                __instance.m_input.text.StartsWith("/") ||
                !ChatChannelState.TryRedirect(__instance.m_input.text))
            {
                return true;
            }
            __instance.m_input.text = "";
            __instance.Hide();
            return false;
        }
    }

    [HarmonyPatch(typeof(Chat), "SendText")]
    internal static class PersistentChatChannelPatch
    {
        private static bool Prefix(Talker.Type type, string text)
        {
            if (type == Talker.Type.Shout)
            {
                SocialChatSender.SendShout(text);
                return false;
            }
            return type != Talker.Type.Normal || !ChatChannelState.TryRedirect(text);
        }
    }

    [HarmonyPatch(typeof(Talker), "Awake")]
    internal static class SocialShoutRangePatch
    {
        private static void Postfix(Talker __instance)
        {
            SocialChatSender.ApplyShoutRange(__instance);
        }
    }

    [HarmonyPatch(typeof(Chat), "Update")]
    internal static class ChatPresentationPatch
    {
        private static Chat owner;
        private static TMP_Text placeholder;
        private static float showUntil;

        private static void Postfix(Chat __instance)
        {
            if (owner != __instance)
            {
                owner = __instance;
                placeholder = null;
            }
            EnsurePlaceholder(__instance);
            if (placeholder != null)
            {
                placeholder.text = ChatChannelState.GetPrompt();
            }
            if (__instance.m_chatWindow != null && Time.time < showUntil)
            {
                __instance.m_chatWindow.gameObject.SetActive(true);
            }
        }

        internal static void Show(Terminal terminal)
        {
            Chat chat = terminal as Chat;
            if (chat == null || chat.HasFocus() || chat.m_chatWindow == null)
            {
                return;
            }
            chat.m_chatWindow.gameObject.SetActive(true);
            showUntil = Time.time + chat.m_hideDelay;
        }

        private static void EnsurePlaceholder(Chat chat)
        {
            if (placeholder != null || chat.m_input == null)
            {
                return;
            }
            TMP_InputField input = chat.m_input.GetComponent<TMP_InputField>() ??
                                   chat.m_input.GetComponentInChildren<TMP_InputField>(true);
            placeholder = input != null ? input.placeholder as TMP_Text : null;
        }
    }

    [HarmonyPatch(typeof(Terminal), "AddString", typeof(string))]
    internal static class AutoDisplaySimpleChatPatch
    {
        private static void Postfix(Terminal __instance) => ChatPresentationPatch.Show(__instance);
    }

    [HarmonyPatch(typeof(Terminal), "AddString", typeof(string), typeof(string), typeof(Talker.Type), typeof(bool))]
    internal static class FormatTitleChatPatch
    {
        private static bool Prefix(Terminal __instance, string title, string text, Talker.Type type, bool timestamp)
        {
            if (type == Talker.Type.Whisper)
            {
                ChatFormatting.AddPrivate(__instance, title, text, timestamp);
                return false;
            }
            if (type == Talker.Type.Shout)
            {
                ChatFormatting.AddShout(__instance, title, text, timestamp);
                return false;
            }
            return true;
        }

        private static void Postfix(Terminal __instance) => ChatPresentationPatch.Show(__instance);
    }

    [HarmonyPatch(typeof(Terminal), "AddString", typeof(PlatformUserID), typeof(string), typeof(Talker.Type), typeof(bool))]
    internal static class FormatUserChatPatch
    {
        private static bool Prefix(Terminal __instance, PlatformUserID user, string text, Talker.Type type, bool timestamp)
        {
            string name = ChatFormatting.GetPlayerName(user);
            if (type == Talker.Type.Whisper)
            {
                ChatFormatting.AddPrivate(__instance, name, text, timestamp);
                return false;
            }
            if (type == Talker.Type.Shout)
            {
                ChatFormatting.AddShout(__instance, name, text, timestamp);
                return false;
            }
            return true;
        }

        private static void Postfix(Terminal __instance) => ChatPresentationPatch.Show(__instance);
    }
}
