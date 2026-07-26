using System.Collections.Generic;
using UnityEngine;

namespace Landoria.Moderator
{
    internal static class ModeratorMapSharing
    {
        private const float RefreshInterval = 30f;
        private static readonly Dictionary<ZDOID, ZNet.PlayerInfo> Players =
            new Dictionary<ZDOID, ZNet.PlayerInfo>();
        private static float _nextRefresh;
        private static bool _enabled;

        internal static void Enable()
        {
            _enabled = true;
            _nextRefresh = 0f;
            ModeratorPlugin.ModLogger.LogDebug("Moderator map player tracking enabled.");
        }

        internal static void Disable()
        {
            if (_enabled || Players.Count > 0)
            {
                ModeratorPlugin.ModLogger.LogDebug("Moderator map player tracking disabled.");
            }
            _enabled = false;
            Players.Clear();
        }

        internal static void Update()
        {
            if (!_enabled || !ModeratorState.IsActive || ZNet.instance == null ||
                Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            RefreshPlayers();
        }

        internal static void AddPlayers(List<ZNet.PlayerInfo> players)
        {
            if (!_enabled || !ModeratorState.IsActive)
            {
                return;
            }

            foreach (ZNet.PlayerInfo player in Players.Values)
            {
                if (!Contains(players, player.m_characterID))
                {
                    players.Add(player);
                }
            }
        }

        private static void RefreshPlayers()
        {
            HashSet<ZDOID> connected = new HashSet<ZDOID>();
            foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
            {
                if (IsLocalPlayer(player.m_characterID) || player.m_characterID.IsNone())
                {
                    continue;
                }
                connected.Add(player.m_characterID);
                RequestPosition(player);
            }
            RemoveDisconnected(connected);
        }

        private static void RequestPosition(ZNet.PlayerInfo player)
        {
            ZDOID characterId = player.m_characterID;
            string playerName = player.m_name;
            PlayerPositionRpc.Request(characterId, playerName, position =>
            {
                if (_enabled && ModeratorState.IsActive)
                {
                    Players[characterId] = CreatePlayerInfo(playerName, characterId, position);
                }
            });
        }

        private static ZNet.PlayerInfo CreatePlayerInfo(
            string name, ZDOID characterId, Vector3 position)
        {
            return new ZNet.PlayerInfo
            {
                m_name = name,
                m_characterID = characterId,
                m_publicPosition = true,
                m_position = position
            };
        }

        private static void RemoveDisconnected(HashSet<ZDOID> connected)
        {
            List<ZDOID> removed = new List<ZDOID>();
            foreach (ZDOID characterId in Players.Keys)
            {
                if (!connected.Contains(characterId))
                {
                    removed.Add(characterId);
                }
            }
            foreach (ZDOID characterId in removed)
            {
                Players.Remove(characterId);
            }
        }

        private static bool Contains(List<ZNet.PlayerInfo> players, ZDOID characterId)
        {
            foreach (ZNet.PlayerInfo player in players)
            {
                if (player.m_characterID == characterId)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsLocalPlayer(ZDOID characterId)
        {
            return ZNet.instance.LocalPlayerCharacterID == characterId;
        }
    }
}
