using System.Collections.Generic;
using HarmonyLib;

namespace Landoria.Parked
{
    internal static class ParkedSession
    {
        private const string IdentityRpc = "Landoria_Parked_Identity";
        private const string SnapshotRpc = "Landoria_Parked_Snapshot";
        private static readonly Dictionary<long, long> PeerPlayers = new Dictionary<long, long>();
        private static readonly Dictionary<long, string> PlayerNames = new Dictionary<long, string>();
        private static readonly HashSet<long> OnlinePlayers = new HashSet<long>();
        private static ZRoutedRpc registeredRpc;
        private static long identityServer;

        internal static void Update()
        {
            RegisterRpcs();
            SendLocalIdentity();
            if (ZNet.instance != null && ZNet.instance.IsServer() && RemoveDisconnectedPeers())
            {
                BroadcastSnapshot();
            }
        }

        internal static void Reset()
        {
            registeredRpc = null;
            identityServer = 0L;
            ClearState();
        }

        private static void ClearState()
        {
            PeerPlayers.Clear();
            PlayerNames.Clear();
            OnlinePlayers.Clear();
        }

        internal static HashSet<long> GetOnlinePlayers()
        {
            return new HashSet<long>(OnlinePlayers);
        }

        internal static long ResolvePeerPlayer(long peer)
        {
            return PeerPlayers.TryGetValue(peer, out long player) ? player : 0L;
        }

        internal static string GetKnownPlayerName(long player)
        {
            return PlayerNames.TryGetValue(player, out string name) ? name : null;
        }

        private static void RegisterRpcs()
        {
            ZRoutedRpc rpc = ZRoutedRpc.instance;
            if (rpc == null || ReferenceEquals(rpc, registeredRpc))
            {
                return;
            }
            rpc.Register<long, string>(IdentityRpc, ReceiveIdentity);
            rpc.Register<ZPackage>(SnapshotRpc, ReceiveSnapshot);
            ClearState();
            registeredRpc = rpc;
            identityServer = 0L;
        }

        private static void SendLocalIdentity()
        {
            ZNet network = ZNet.instance;
            Player player = Player.m_localPlayer;
            if (network == null || player == null || network.IsServer() || registeredRpc == null)
            {
                return;
            }
            ZNetPeer server = network.GetServerPeer();
            if (server == null || server.m_uid == identityServer)
            {
                return;
            }
            identityServer = server.m_uid;
            registeredRpc.InvokeRoutedRPC(
                server.m_uid, IdentityRpc, player.GetPlayerID(), player.GetPlayerName());
        }

        private static void ReceiveIdentity(long sender, long playerId, string playerName)
        {
            if (ZNet.instance == null || !ZNet.instance.IsServer() ||
                ZNet.instance.GetPeer(sender) == null || playerId == 0L)
            {
                return;
            }
            PeerPlayers[sender] = playerId;
            PlayerNames[playerId] = playerName ?? string.Empty;
            OnlinePlayers.Add(playerId);
            BroadcastSnapshot();
        }

        private static bool RemoveDisconnectedPeers()
        {
            bool changed = false;
            foreach (long peer in new List<long>(PeerPlayers.Keys))
            {
                if (ZNet.instance.GetPeer(peer)?.IsReady() == true)
                {
                    continue;
                }
                OnlinePlayers.Remove(PeerPlayers[peer]);
                PeerPlayers.Remove(peer);
                changed = true;
            }
            return changed;
        }

        private static void BroadcastSnapshot()
        {
            if (registeredRpc == null)
            {
                return;
            }
            ZPackage package = new ZPackage();
            WriteMappings(package);
            registeredRpc.InvokeRoutedRPC(ZRoutedRpc.Everybody, SnapshotRpc, package);
        }

        private static void WriteMappings(ZPackage package)
        {
            package.Write(PeerPlayers.Count);
            foreach (KeyValuePair<long, long> mapping in PeerPlayers)
            {
                package.Write(mapping.Key);
                package.Write(mapping.Value);
                package.Write(GetKnownPlayerName(mapping.Value) ?? string.Empty);
            }
        }

        private static void ReceiveSnapshot(long sender, ZPackage package)
        {
            if (!IsTrustedServer(sender))
            {
                return;
            }
            PeerPlayers.Clear();
            OnlinePlayers.Clear();
            int count = package.ReadInt();
            for (int index = 0; index < count; index++)
            {
                long peer = package.ReadLong();
                long player = package.ReadLong();
                PeerPlayers[peer] = player;
                OnlinePlayers.Add(player);
                PlayerNames[player] = package.ReadString();
            }
        }

        private static bool IsTrustedServer(long sender)
        {
            ZNet network = ZNet.instance;
            if (network == null || network.IsServer())
            {
                return network != null && network.IsServer();
            }
            return network.GetServerPeer()?.m_uid == sender;
        }

        [HarmonyPatch(typeof(Player), "OnSpawned")]
        private static class PlayerSpawnPatch
        {
            private static void Postfix(Player __instance)
            {
                if (__instance == Player.m_localPlayer)
                {
                    identityServer = 0L;
                    Update();
                }
            }
        }
    }
}
