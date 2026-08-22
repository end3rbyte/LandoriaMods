extern alias ModSentryApi;

using System;
using System.Collections;
using HarmonyLib;
using ModSentryGuestMarker =
    ModSentryApi::Landoria.ModSentry.ModSentryGuestMarker;
using UnityEngine;

namespace Landoria.LandoriaModPack
{
    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    internal static class OnPlayerSpawnedPatch
    {
        private const float DelaySeconds = 5f;
        private static ZNet _scheduledNetwork;

        [HarmonyPostfix]
        private static void Postfix(Player __instance)
        {
            ZNet network = ZNet.instance;
            if (network == null || ReferenceEquals(_scheduledNetwork, network) ||
                __instance != Player.m_localPlayer || IsLocalGuest())
            {
                return;
            }
            _scheduledNetwork = network;
            LandoriaModPackPlugin.Run(FirstSpawnAfterIntro(network));
        }

        private static IEnumerator FirstSpawnAfterIntro(ZNet network)
        {
            yield return new WaitUntil(() =>
                !ReferenceEquals(ZNet.instance, network) || HasPassedIntro());
            if (!ReferenceEquals(ZNet.instance, network) || IsLocalGuest())
            {
                yield break;
            }
            yield return new WaitForSeconds(DelaySeconds);
            yield return LandoriaWebsitePopup.ShowWhenAvailable(() =>
                !ReferenceEquals(ZNet.instance, network) || IsLocalGuest());
        }

        private static bool HasPassedIntro()
        {
            Player player = Player.m_localPlayer;
            return player != null && !player.InIntro() &&
                (Game.instance == null || !Game.instance.InIntro(includeQueued: true));
        }

        private static bool IsLocalGuest()
        {
            ZNetPeer serverPeer = ZNet.instance?.GetServerPeer();
            return serverPeer?.m_rpc != null &&
                ModSentryGuestMarker.IsMarked(serverPeer.m_rpc);
        }
    }
}
