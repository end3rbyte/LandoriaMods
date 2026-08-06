using System;
using UnityEngine;

namespace Landoria.Socialize
{
    internal static class TargetPingService
    {
        private const string RequestRpc = "Landoria_Social_TargetPing";
        private const string MessageRpc = "Landoria_Social_TargetPingMessage";
        private static ZRoutedRpc registeredRpc;

        internal static void Update()
        {
            RpcRegistry.RegisterIfChanged(ref registeredRpc, RegisterRpcs);
        }

        internal static void Reset()
        {
            registeredRpc = null;
        }

        internal static bool Send(string targetName, string message, Terminal context)
        {
            if (!SocializePlugin.IsEnabled ||
                !TryFindTarget(targetName, out ZNet.PlayerInfo target) ||
                Player.m_localPlayer == null || ZRoutedRpc.instance == null)
            {
                context?.AddString("No connected player named \"" + targetName + "\" was found.");
                return false;
            }
            EnsureRpcs();
            Vector3 position = Player.m_localPlayer.GetHeadPoint();
            Chat.GetChatMessageData(message, true, out UserInfo user, out string filtered);
            if (target.m_characterID.UserID != ZNet.instance.LocalPlayerCharacterID.UserID)
            {
                SendRequest(position, user, target.m_characterID.UserID, filtered);
            }
            ShowLocalPing(position, user, target.m_name, filtered, context);
            return true;
        }

        internal static void RPC_TargetPing(
            long sender,
            Vector3 position,
            UserInfo user,
            long targetPlayerId,
            string message)
        {
            if (!SocializePlugin.IsEnabled || ZNet.instance == null ||
                !ZNet.instance.IsServer())
            {
                return;
            }
            ZRoutedRpc.instance.InvokeRoutedRPC(
                targetPlayerId,
                "ChatMessage",
                position,
                (int)Talker.Type.Ping,
                user,
                message);
            ZRoutedRpc.instance.InvokeRoutedRPC(targetPlayerId, MessageRpc, user, message);
        }

        internal static void RPC_TargetPingMessage(long sender, UserInfo user, string message)
        {
            if (SocializePlugin.IsEnabled)
            {
                ChatFormatting.AddPing(Chat.instance, user.GetDisplayName(), "", message);
            }
        }

        private static void EnsureRpcs()
        {
            RpcRegistry.RegisterIfChanged(ref registeredRpc, RegisterRpcs);
        }

        private static void RegisterRpcs(ZRoutedRpc rpc)
        {
            rpc.Register<Vector3, UserInfo, long, string>(RequestRpc, RPC_TargetPing);
            rpc.Register<UserInfo, string>(MessageRpc, RPC_TargetPingMessage);
        }

        private static void SendRequest(Vector3 position, UserInfo user, long target, string message)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RequestRpc, position, user, target, message);
        }

        private static void ShowLocalPing(
            Vector3 position,
            UserInfo user,
            string targetName,
            string message,
            Terminal context)
        {
            long localPlayerId = ZNet.instance.LocalPlayerCharacterID.UserID;
            ZRoutedRpc.instance.InvokeRoutedRPC(
                localPlayerId,
                "ChatMessage",
                position,
                (int)Talker.Type.Ping,
                user,
                message);
            ChatFormatting.AddPing(context, user.GetDisplayName(), targetName, message);
        }

        private static bool TryFindTarget(string targetName, out ZNet.PlayerInfo target)
        {
            foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
            {
                if (string.Equals(player.m_name, targetName, StringComparison.OrdinalIgnoreCase))
                {
                    target = player;
                    return true;
                }
            }
            target = default;
            return false;
        }
    }
}
