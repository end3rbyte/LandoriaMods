using System.Collections.Generic;
using UnityEngine;

namespace Landoria.Socialize
{
    internal static class GroupMapSharing
    {
        private static readonly Dictionary<long, ZNet.PlayerInfo> Players =
            new Dictionary<long, ZNet.PlayerInfo>();

        internal static void Clear()
        {
            Players.Clear();
        }

        internal static void WritePosition(ZPackage package, long playerId)
        {
            if (!TryGetPosition(playerId, out ZDOID id, out Vector3 position))
            {
                package.Write(false);
                return;
            }
            package.Write(true);
            package.Write(id);
            package.Write(position);
        }

        internal static void ReadPosition(ZPackage package, long playerId, string name)
        {
            if (!package.ReadBool())
            {
                Players.Remove(playerId);
                return;
            }
            Players[playerId] = new ZNet.PlayerInfo
            {
                m_name = name,
                m_characterID = package.ReadZDOID(),
                m_publicPosition = true,
                m_position = package.ReadVector3()
            };
        }

        internal static void AddGroupMembers(List<ZNet.PlayerInfo> players)
        {
            foreach (KeyValuePair<long, ZNet.PlayerInfo> member in Players)
            {
                if (!IsLocalPlayer(member.Key) && !Contains(players, member.Key))
                {
                    players.Add(member.Value);
                }
            }
        }

        private static bool TryGetPosition(
            long playerId, out ZDOID characterId, out Vector3 position)
        {
            Player player = Player.GetPlayer(playerId);
            if (player != null)
            {
                characterId = player.GetZDOID();
                position = player.transform.position;
                return true;
            }
            return TryGetPeerPosition(playerId, out characterId, out position);
        }

        private static bool TryGetPeerPosition(
            long playerId, out ZDOID characterId, out Vector3 position)
        {
            foreach (ZNetPeer peer in ZNet.instance.GetPeers())
            {
                if (GroupState.PeerPlayers.TryGetValue(peer.m_uid, out long mapped)
                    && mapped == playerId && peer.IsReady() && !peer.m_characterID.IsNone())
                {
                    characterId = peer.m_characterID;
                    position = peer.m_refPos;
                    return true;
                }
            }
            characterId = ZDOID.None;
            position = Vector3.zero;
            return false;
        }

        private static bool Contains(List<ZNet.PlayerInfo> players, long playerId)
        {
            foreach (ZNet.PlayerInfo player in players)
            {
                if (player.m_characterID.UserID == playerId) return true;
            }
            return false;
        }

        private static bool IsLocalPlayer(long playerId)
        {
            return Game.instance != null
                   && Game.instance.GetPlayerProfile().GetPlayerID() == playerId;
        }
    }
}
