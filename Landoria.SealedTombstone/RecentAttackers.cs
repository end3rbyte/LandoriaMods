using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Landoria.SealedTombstone
{
    internal static class RecentAttackers
    {
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(2);
        private static readonly Dictionary<long, DateTime> LastHits =
            new Dictionary<long, DateTime>();

        private static long[] _deathSnapshot = new long[0];

        internal static void Record(Player victim, HitData hit)
        {
            Player attacker = hit?.GetAttacker() as Player;
            if (victim != Player.m_localPlayer || attacker == null)
            {
                return;
            }

            long attackerId = attacker.GetPlayerID();
            if (attackerId != 0L && attackerId != victim.GetPlayerID())
            {
                LastHits[attackerId] = DateTime.UtcNow;
            }
        }

        internal static void SnapshotForDeath(Player player)
        {
            if (player != Player.m_localPlayer)
            {
                return;
            }

            DateTime threshold = DateTime.UtcNow - Window;
            _deathSnapshot = LastHits.Where(entry => entry.Value >= threshold)
                .Select(entry => entry.Key).ToArray();
            LastHits.Clear();
            SealedTombstonePlugin.Log.LogDebug($"Captured {_deathSnapshot.Length} recent PvP attackers.");
        }

        internal static string ConsumeSnapshot()
        {
            string value = string.Join(",", _deathSnapshot.Select(id =>
                id.ToString(CultureInfo.InvariantCulture)));
            _deathSnapshot = new long[0];
            return value;
        }

        internal static bool Contains(string serializedIds, long playerId)
        {
            if (string.IsNullOrEmpty(serializedIds))
            {
                return false;
            }

            string expected = playerId.ToString(CultureInfo.InvariantCulture);
            return serializedIds.Split(',').Any(id => id == expected);
        }

        internal static void Reset()
        {
            LastHits.Clear();
            _deathSnapshot = new long[0];
        }
    }
}
