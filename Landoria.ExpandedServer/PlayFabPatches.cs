using HarmonyLib;
using PlayFab;
using PlayFab.MultiplayerModels;
using PlayFab.Party;

namespace Landoria.ExpandedServer
{
    [HarmonyPatch(typeof(PlayFabMultiplayerAPI), "CreateLobby")]
    internal static class IncreasePlayFabLobbyPlayerLimitPatch
    {
        private static void Prefix(CreateLobbyRequest request)
        {
            if (request != null && ExpandedServerPlugin.IsLocalServer &&
                request.MaxPlayers != ExpandedServerPlugin.MaxPlayers)
            {
                request.MaxPlayers = (uint)ExpandedServerPlugin.MaxPlayers;
                ExpandedServerPlugin.Log.LogDebug($"PlayFab lobby capacity set to {request.MaxPlayers} players.");
            }
        }
    }

    [HarmonyPatch(typeof(PlayFabMultiplayerManager), "CreateAndJoinNetwork",
        typeof(PlayFabNetworkConfiguration))]
    internal static class IncreasePlayFabNetworkPlayerLimitPatch
    {
        private static void Prefix(PlayFabNetworkConfiguration networkConfiguration)
        {
            if (networkConfiguration != null && ExpandedServerPlugin.IsLocalServer &&
                networkConfiguration.MaxPlayerCount != ExpandedServerPlugin.MaxPlayers)
            {
                networkConfiguration.MaxPlayerCount = (uint)ExpandedServerPlugin.MaxPlayers;
                ExpandedServerPlugin.Log.LogDebug(
                    $"PlayFab network capacity set to {networkConfiguration.MaxPlayerCount} players.");
            }
        }
    }
}
