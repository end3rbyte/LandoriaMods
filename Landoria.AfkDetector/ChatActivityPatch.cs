using System;
using HarmonyLib;

namespace Landoria.AfkDetector
{
    [HarmonyPatch(typeof(ZRoutedRpc), "RPC_RoutedRPC")]
    internal static class ChatActivityPatch
    {
        private static readonly int ChatMessageHash =
            StringExtensionMethods.GetStableHashCode("ChatMessage", true);
        private static readonly int SayHash = StringExtensionMethods.GetStableHashCode("Say", true);
        private static readonly int GroupRequestHash =
            StringExtensionMethods.GetStableHashCode("Landoria_Social_GroupRequest", true);

        private static void Prefix(ZPackage pkg)
        {
            if (AfkDetectorPlugin.Instance == null || ZNet.instance == null || !ZNet.instance.IsServer())
            {
                return;
            }
            try
            {
                ZRoutedRpc.RoutedRPCData data = ReadRoutedData(pkg);
                if (ContainsChatMessage(data))
                {
                    AfkDetectorPlugin.Instance.RecordChat(data.m_senderPeerID);
                }
            }
            catch (Exception exception)
            {
                AfkDetectorPlugin.Log.LogDebug($"Ignored unreadable chat activity: {exception.Message}");
            }
        }

        private static ZRoutedRpc.RoutedRPCData ReadRoutedData(ZPackage source)
        {
            ZPackage copy = new ZPackage(source.GetArray());
            ZRoutedRpc.RoutedRPCData data = new ZRoutedRpc.RoutedRPCData();
            data.Deserialize(copy);
            return data;
        }

        private static bool ContainsChatMessage(ZRoutedRpc.RoutedRPCData data)
        {
            ZPackage parameters = new ZPackage(data.m_parameters.GetArray());
            if (data.m_methodHash == ChatMessageHash)
            {
                return ReadChatMessage(parameters, hasPosition: true);
            }
            if (data.m_methodHash == SayHash)
            {
                return ReadChatMessage(parameters, hasPosition: false);
            }
            return data.m_methodHash == GroupRequestHash && ReadGroupChat(parameters);
        }

        private static bool ReadChatMessage(ZPackage package, bool hasPosition)
        {
            if (hasPosition)
            {
                package.ReadVector3();
            }
            package.ReadInt();
            package.ReadString();
            package.ReadString();
            return !string.IsNullOrWhiteSpace(package.ReadString());
        }

        private static bool ReadGroupChat(ZPackage parameters)
        {
            ZPackage request = parameters.ReadPackage();
            string action = request.ReadString();
            request.ReadLong();
            request.ReadString();
            return action == "chat" && !string.IsNullOrWhiteSpace(request.ReadString());
        }
    }
}
