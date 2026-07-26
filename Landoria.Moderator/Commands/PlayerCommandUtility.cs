using System;
using UnityEngine;

namespace Landoria.Moderator
{
    internal static class PlayerCommandUtility
    {
        internal static bool TryFindPlayer(string name, out Vector3 position, out ZDOID characterId)
        {
            position = Vector3.zero;
            characterId = ZDOID.None;
            if (ZNet.instance == null)
            {
                return false;
            }

            foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
            {
                if (!player.m_name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                position = player.m_position;
                characterId = player.m_characterID;
                return true;
            }

            return false;
        }

        internal static void Teleport(ZDOID characterId, Vector3 position, Quaternion rotation)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(
                0L, characterId, "RPC_TeleportTo", position, rotation, true);
        }
    }
}
