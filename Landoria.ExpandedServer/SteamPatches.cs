using HarmonyLib;
using Steamworks;

namespace Landoria.ExpandedServer
{
    [HarmonyPatch(typeof(SteamMatchmaking), "CreateLobby",
        typeof(ELobbyType), typeof(int))]
    internal static class SetSteamLobbyPlayerLimitPatch
    {
        private static void Prefix(ref int cMaxMembers)
        {
            if (ExpandedServerPlugin.IsLocalServer &&
                cMaxMembers != ExpandedServerPlugin.MaxPlayers)
            {
                cMaxMembers = ExpandedServerPlugin.MaxPlayers;
                ExpandedServerPlugin.Log.LogDebug(
                    $"Steam lobby capacity set to {cMaxMembers} players.");
            }
        }
    }
}
