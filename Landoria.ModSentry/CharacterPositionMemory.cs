using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Landoria.ModSentry
{
    internal static class CharacterPositionMemory
    {
        private const string Key = "Landoria.ModSentry.LastVerifiedPosition";

        internal static void Save(Player player)
        {
            if (player == null || player != Player.m_localPlayer ||
                !ClientVerificationState.IsAccepted)
            {
                return;
            }
            Vector3 position = player.transform.position;
            player.m_customData[Key] = string.Join("|",
                position.x.ToString("R", CultureInfo.InvariantCulture),
                position.y.ToString("R", CultureInfo.InvariantCulture),
                position.z.ToString("R", CultureInfo.InvariantCulture));
        }

        internal static bool TryLoad(Player player, out Vector3 position)
        {
            position = default;
            if (player == null || !player.m_customData.TryGetValue(Key,
                    out string stored))
            {
                return false;
            }
            string[] parts = stored.Split('|');
            return parts.Length == 3 && TryParse(parts, out position);
        }

        private static bool TryParse(string[] parts, out Vector3 position)
        {
            position = default;
            if (!float.TryParse(parts[0], NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float x) ||
                !float.TryParse(parts[1], NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float y) ||
                !float.TryParse(parts[2], NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float z))
            {
                return false;
            }
            position = new Vector3(x, y, z);
            return IsFinite(position);
        }

        internal static bool IsFinite(Vector3 position) =>
            IsFinite(position.x) && IsFinite(position.y) && IsFinite(position.z);

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);
    }

    internal static class ClientVerificationState
    {
        private static ZRpc _serverRpc;
        private static bool _positionSent;

        internal static bool IsAccepted { get; private set; }

        internal static void Begin(ZRpc serverRpc)
        {
            Clear();
            _serverRpc = serverRpc;
        }

        internal static void Accept(ZRpc rpc)
        {
            _serverRpc = rpc;
            IsAccepted = true;
        }

        internal static void Update()
        {
            if (!IsAccepted || _positionSent || _serverRpc == null ||
                !CharacterPositionMemory.TryLoad(Player.m_localPlayer,
                    out Vector3 position))
            {
                return;
            }
            ZPackage package = new ZPackage();
            package.Write(position);
            _serverRpc.Invoke(ModSentryPlugin.CharacterPositionRpc, package);
            _positionSent = true;
        }

        internal static void Clear()
        {
            _serverRpc = null;
            _positionSent = false;
            IsAccepted = false;
        }
    }

    internal static class VerifiedCharacterPositions
    {
        private static readonly Dictionary<ZRpc, Vector3> Positions =
            new Dictionary<ZRpc, Vector3>();

        internal static void Receive(ZRpc rpc, ZPackage package)
        {
            if (!HandshakeState.IsAccepted(rpc))
            {
                return;
            }
            Vector3 position = package.ReadVector3();
            if (CharacterPositionMemory.IsFinite(position))
            {
                Positions[rpc] = position;
            }
        }

        internal static bool TryGet(ZRpc rpc, out Vector3 position) =>
            Positions.TryGetValue(rpc, out position);

        internal static void Remove(ZRpc rpc) => Positions.Remove(rpc);

        internal static void Clear() => Positions.Clear();
    }
}
