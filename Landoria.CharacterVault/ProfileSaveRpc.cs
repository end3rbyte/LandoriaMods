using HarmonyLib;

namespace Landoria.CharacterVault
{
    [HarmonyPatch(typeof(ZNet), "Start")]
    internal static class PendingExitRequestPatch
    {
        private static void Postfix()
        {
            CharacterVaultPlugin.Coordinator?.ProcessPendingExitRequest();
        }
    }

    [HarmonyPatch(typeof(ZNet), "OnNewConnection")]
    internal static class CharacterVaultConnectionPatch
    {
        private static void Postfix(ZNet __instance, ZNetPeer peer)
        {
            CharacterVaultPlugin.Transfers?.Register(__instance, peer);
        }
    }

    [HarmonyPatch(typeof(ZNet), "SendPeerInfo")]
    [HarmonyAfter("Landoria.ModSentry")]
    internal static class CharacterVaultHelloPatch
    {
        private static void Prefix(ZRpc rpc)
        {
            if (ZNet.instance?.IsServer() == false)
            {
                CharacterVaultPlugin.Transfers?.SendHello(rpc);
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
    [HarmonyAfter("Landoria.ModSentry")]
    internal static class CharacterVaultAdmissionBarrierPatch
    {
        private static void Prefix(ZRpc rpc, bool __runOriginal)
        {
            if (__runOriginal && ZNet.instance?.IsServer() == true)
            {
                CharacterVaultPlugin.Transfers?.Approve(rpc);
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), "Disconnect")]
    internal static class CharacterVaultDisconnectPatch
    {
        private static void Prefix(ZNetPeer peer)
        {
            CharacterVaultPlugin.Transfers?.Remove(peer);
        }
    }

    [HarmonyPatch(typeof(PlayerProfile), "SavePlayerToDisk")]
    internal static class CharacterVaultProfileSavedPatch
    {
        private static void Postfix(PlayerProfile __instance, bool __result)
        {
            if (__result)
            {
                CharacterVaultPlugin.Transfers?.UploadSavedProfile(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Player), "OnSpawned")]
    internal static class CharacterVaultStartingItemsPatch
    {
        private static void Postfix()
        {
            CharacterVaultPlugin.Transfers?.GrantStartingItems();
        }
    }

    [HarmonyPatch(typeof(Game), "SpawnPlayer")]
    internal static class CharacterVaultApplyProfilePatch
    {
        private static void Prefix(ref PlayerProfile ___m_playerProfile)
        {
            CharacterVaultPlugin.Transfers?.ApplyPendingProfile(ref ___m_playerProfile);
        }
    }

    [HarmonyPatch(typeof(ZNet), "Save")]
    internal static class CharacterVaultWorldSavePatch
    {
        private static void Prefix(ZNet __instance)
        {
            if (__instance.IsServer())
            {
                CharacterVaultPlugin.Transfers?.RequestWorldCheckpoint();
            }
        }
    }
}
