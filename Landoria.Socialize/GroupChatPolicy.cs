using System;

namespace Landoria.Socialize
{
    internal sealed class GroupChatResult
    {
        internal GroupChatResult(bool broadcast, string message)
        {
            Broadcast = broadcast;
            Message = message;
        }

        internal bool Broadcast { get; }
        internal string Message { get; }
    }

    internal static class GroupChatPolicy
    {
        internal static GroupChatResult Prepare(SocialGroup group, long actor, string message,
            Func<long, bool> isOnline, Func<string, string, string> format)
        {
            if (group == null)
            {
                return Reject("You are not in a group.");
            }
            foreach (long member in group.Members.Keys)
            {
                if (member != actor && isOnline(member))
                {
                    return new GroupChatResult(
                        true, format(group.Members[actor], message));
                }
            }
            return Reject("No other group member is connected.");
        }

        private static GroupChatResult Reject(string message)
        {
            return new GroupChatResult(false, message);
        }
    }
}
