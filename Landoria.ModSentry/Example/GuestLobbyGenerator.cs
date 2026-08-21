using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GuestLobbyExample
{
    /// <summary>Locates, builds, and recreates the guest lobby.</summary>
    internal static class GuestLobbyGenerator
    {
        private const string WorldKey = "example.guest_lobby";
        private const string FloorPrefab = "stone_floor_2x2";
        private const string WallPrefab = "iron_wall_2x2";
        private const string VentCoverPrefab = "darkwood_roof_45";
        private const string BeamPrefab = "wood_beam";
        private const string SignPrefab = "sign";
        private const string GroundTorchPrefab = "CastleKit_groundtorch";
        private const string HangingBrazierPrefab = "piece_brazierceiling01";
        private const string DeerRugPrefab = "rug_deer";
        private const string LobbySignText = "Guest Lobby";
        private const string RequirementSignText =
            "You need ModSentry to enter this world";
        private const string DownloadSignText =
            "https://thunderstore.io/c/valheim/p/Landoria/ModSentry/";
        private const float FloorClearance = 70f;
        private const float HorizontalOffsetFromStartTemple = 40f;
        private const float GroundScanRadius = 40f;
        private const float GroundScanStep = 5f;
        private const float TileSize = 2f;
        private const float RoofWallOverlap = 0.1f;
        private const int Width = 5;
        private const int WallRows = 4;
        /// <summary>Gets whether the lobby was generated successfully.</summary>
        internal static bool IsOperational { get; private set; }

        /// <summary>Attaches lobby generation to Valheim's location setup.</summary>
        internal static void Attach()
        {
            if (ZoneSystem.instance == null || ZNet.instance?.IsServer() != true ||
                IsOperational)
            {
                return;
            }
            GuestLobbyPlugin.Log.LogInfo(
                "Guest lobby generation is waiting for world locations.");
            ZoneSystem.instance.GenerateLocationsCompleted -= Generate;
            ZoneSystem.instance.GenerateLocationsCompleted += Generate;
        }

        /// <summary>Resolves the saved guest spawn position.</summary>
        internal static bool TryGetPosition(out Vector3 position)
        {
            position = default;
            if (ZoneSystem.instance == null ||
                !ZoneSystem.instance.GetGlobalKey(WorldKey, out string stored))
            {
                return false;
            }
            string[] parts = stored.Split(',');
            return parts.Length == 3 && TryParse(parts, out position);
        }

        /// <summary>Resolves a safe fallback outside the Guest Lobby.</summary>
        internal static bool TryGetNormalPosition(out Vector3 position)
        {
            position = default;
            if (Game.instance == null || ZoneSystem.instance == null ||
                !ZoneSystem.instance.GetLocationIcon(
                    Game.instance.m_StartLocation, out position))
            {
                return false;
            }
            position.y = ZoneSystem.instance.GetGroundHeight(position) + 1f;
            return true;
        }

        private static void Generate()
        {
            ZoneSystem.instance.GenerateLocationsCompleted -= Generate;
            RemovePreviousLobby();
            if (!TryGetFloorPosition(out Vector3 floor) || !Build(floor))
            {
                IsOperational = false;
                GuestLobbyPlugin.Log.LogError("Guest lobby generation failed.");
                return;
            }
            SavePosition(floor + Vector3.up);
            IsOperational = true;
            GuestLobbyPlugin.Log.LogInfo("Guest lobby generation completed.");
        }

        private static bool Build(Vector3 center)
        {
            GameObject floor = ZNetScene.instance?.GetPrefab(FloorPrefab);
            GameObject wall = ZNetScene.instance?.GetPrefab(WallPrefab);
            GameObject ventCover =
                ZNetScene.instance?.GetPrefab(VentCoverPrefab);
            GameObject beam = ZNetScene.instance?.GetPrefab(BeamPrefab);
            GameObject sign = ZNetScene.instance?.GetPrefab(SignPrefab);
            GameObject groundTorch =
                ZNetScene.instance?.GetPrefab(GroundTorchPrefab);
            GameObject hangingBrazier =
                ZNetScene.instance?.GetPrefab(HangingBrazierPrefab);
            GameObject deerRug = ZNetScene.instance?.GetPrefab(DeerRugPrefab);
            if (!IsNetworkPrefab(floor) || !IsNetworkPrefab(wall) ||
                !IsNetworkPrefab(ventCover) || !IsNetworkPrefab(beam) ||
                !IsNetworkPrefab(sign) || !IsNetworkPrefab(groundTorch) ||
                !IsNetworkPrefab(hangingBrazier) || !IsNetworkPrefab(deerRug))
            {
                return false;
            }
            float surface = SpawnFloor(floor, center);
            float ceiling = SpawnWalls(wall, center, surface) -
                RoofWallOverlap;
            SpawnRoof(floor, center, ceiling);
            SpawnVentCover(ventCover, center, ceiling);
            float suspensionHeight = SpawnCrossedBeams(beam, center, ceiling);
            SpawnSigns(sign, center, surface);
            SpawnSignTorch(groundTorch, center, surface);
            SpawnHangingBrazier(hangingBrazier, center, suspensionHeight);
            SpawnDeerRug(deerRug, center, surface);
            return true;
        }

        private static float SpawnFloor(GameObject prefab, Vector3 center)
        {
            float first = -(Width - 1) * TileSize * 0.5f;
            float surface = float.MinValue;
            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Width; z++)
                {
                    Vector3 offset = new Vector3(first + x * TileSize, 0f,
                        first + z * TileSize);
                    GameObject floor = Spawn(prefab, center + offset,
                        Quaternion.identity);
                    surface = VisualTop(floor, surface);
                }
            }
            return surface;
        }

        private static void SpawnRoof(GameObject prefab, Vector3 center,
            float ceiling)
        {
            float first = -(Width - 1) * TileSize * 0.5f;
            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Width; z++)
                {
                    if (x == Width / 2 && z == Width / 2)
                    {
                        continue;
                    }
                    Vector3 offset = new Vector3(first + x * TileSize, 0f,
                        first + z * TileSize);
                    SpawnGrounded(prefab, center + offset, ceiling);
                }
            }
        }

        private static void SpawnVentCover(GameObject prefab, Vector3 center,
            float ceiling)
        {
            SpawnGrounded(prefab, center, ceiling);
        }

        private static float SpawnCrossedBeams(GameObject prefab,
            Vector3 center, float ceiling)
        {
            GameObject acrossX = SpawnGrounded(prefab, center,
                Quaternion.identity, ceiling);
            GameObject acrossZ = SpawnGrounded(prefab, center,
                Quaternion.Euler(0f, 90f, 0f), ceiling);
            return Mathf.Min(VisualBottom(acrossX), VisualBottom(acrossZ));
        }

        private static float SpawnWalls(GameObject prefab, Vector3 center,
            float surface)
        {
            float edge = Width * TileSize * 0.5f;
            float first = -(Width - 1) * TileSize * 0.5f;
            float wallTop = surface;
            for (int row = 0; row < WallRows; row++)
            {
                float rowBottom = surface + row * TileSize;
                for (int index = 0; index < Width; index++)
                {
                    float offset = first + index * TileSize;
                    wallTop = Mathf.Max(wallTop, SpawnWallPair(prefab,
                        center + new Vector3(offset, 0f, -edge),
                        center + new Vector3(offset, 0f, edge),
                        Quaternion.identity, rowBottom));
                    wallTop = Mathf.Max(wallTop, SpawnWallPair(prefab,
                        center + new Vector3(-edge, 0f, offset),
                        center + new Vector3(edge, 0f, offset),
                        Quaternion.Euler(0f, 90f, 0f), rowBottom));
                }
            }
            return wallTop;
        }

        private static float SpawnWallPair(GameObject prefab, Vector3 first,
            Vector3 second, Quaternion rotation, float bottom)
        {
            return Mathf.Max(SpawnWall(prefab, first, rotation, bottom),
                SpawnWall(prefab, second, rotation, bottom));
        }

        private static float SpawnWall(GameObject prefab, Vector3 position,
            Quaternion rotation, float bottom)
        {
            GameObject wall = SpawnGrounded(prefab, position, rotation, bottom);
            return VisualTop(wall, bottom);
        }

        private static void SpawnSigns(GameObject prefab, Vector3 center,
            float surface)
        {
            const float middleHeight = 1.7f;
            const float centerSpacing = 1.5f;
            SpawnSign(prefab, center, -centerSpacing, surface + middleHeight,
                LobbySignText);
            SpawnSign(prefab, center, 0f, surface + middleHeight,
                RequirementSignText);
            SpawnSign(prefab, center, centerSpacing, surface + middleHeight,
                DownloadSignText);
        }

        private static void SpawnSign(GameObject prefab, Vector3 center,
            float horizontalOffset, float height, string text)
        {
            Vector3 position = new Vector3(center.x + horizontalOffset, height,
                center.z + Width * TileSize * 0.5f - 0.15f);
            GameObject sign = Object.Instantiate(prefab, position,
                Quaternion.Euler(0f, 180f, 0f));
            ZNetView view = sign.GetComponent<ZNetView>();
            GuestLobbyProtection.MarkSign(view, text);
            GuestLobbyProtection.MarkAndApply(view);
        }

        private static void SpawnSignTorch(GameObject prefab,
            Vector3 center, float surface)
        {
            float signWall = Width * TileSize * 0.5f;
            Vector3 position = new Vector3(center.x, center.y,
                center.z + signWall - 3f);
            SpawnGrounded(prefab, position, surface);
        }

        private static void SpawnHangingBrazier(GameObject prefab,
            Vector3 center, float ceiling)
        {
            GameObject brazier = Object.Instantiate(prefab, center,
                Quaternion.identity);
            AlignTopAndSynchronize(brazier, ceiling);
            ZNetView view = brazier.GetComponent<ZNetView>();
            GuestLobbyProtection.MarkAndApply(view);
        }

        private static GameObject SpawnGrounded(GameObject prefab,
            Vector3 position,
            float surface)
        {
            return SpawnGrounded(prefab, position, Quaternion.identity,
                surface);
        }

        private static GameObject SpawnGrounded(GameObject prefab,
            Vector3 position, Quaternion rotation, float surface)
        {
            GameObject instance = Spawn(prefab, position, rotation);
            AlignVisualBottom(instance, surface);
            SynchronizePosition(instance);
            return instance;
        }

        private static void SpawnDeerRug(GameObject prefab, Vector3 center,
            float surface)
        {
            SpawnGrounded(prefab, center, surface);
        }

        private static GameObject Spawn(GameObject prefab, Vector3 position,
            Quaternion rotation)
        {
            GameObject instance = Object.Instantiate(prefab, position, rotation);
            GuestLobbyProtection.MarkAndApply(
                instance.GetComponent<ZNetView>());
            return instance;
        }

        private static void AlignVisualTop(GameObject instance, float ceiling)
        {
            float top = VisualTop(instance, float.MinValue);
            if (top > float.MinValue)
            {
                instance.transform.position += Vector3.up * (ceiling - top);
            }
        }

        private static void AlignTopAndSynchronize(GameObject instance,
            float ceiling)
        {
            AlignVisualTop(instance, ceiling);
            SynchronizePosition(instance);
        }

        private static void AlignVisualBottom(GameObject instance,
            float surface)
        {
            float bottom = VisualBottom(instance);
            if (bottom < float.MaxValue)
            {
                instance.transform.position += Vector3.up * (surface - bottom);
            }
        }

        private static float VisualBottom(GameObject instance)
        {
            float bottom = float.MaxValue;
            foreach (MeshRenderer renderer in
                     instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                bottom = Mathf.Min(bottom, renderer.bounds.min.y);
            }
            foreach (SkinnedMeshRenderer renderer in
                     instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                bottom = Mathf.Min(bottom, renderer.bounds.min.y);
            }
            return bottom;
        }

        private static void SynchronizePosition(GameObject instance)
        {
            instance.GetComponent<ZNetView>()?.GetZDO()?.SetPosition(
                instance.transform.position);
        }

        private static float VisualTop(GameObject instance, float fallback)
        {
            float top = fallback;
            foreach (MeshRenderer renderer in
                     instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                top = Mathf.Max(top, renderer.bounds.max.y);
            }
            foreach (SkinnedMeshRenderer renderer in
                     instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                top = Mathf.Max(top, renderer.bounds.max.y);
            }
            return top;
        }

        private static void RemovePreviousLobby()
        {
            if (ZDOMan.instance == null || ZoneSystem.instance == null)
            {
                return;
            }
            List<ZDOID> ids = ZDOExtraData.GetAllZDOIDsWithHash(
                ZDOExtraData.Type.Int,
                GuestLobbyUtility.StableHash(GuestLobbyProtection.Marker));
            foreach (ZDOID id in ids)
            {
                ZDO zdo = ZDOMan.instance.GetZDO(id);
                if (GuestLobbyProtection.IsProtected(zdo))
                {
                    zdo.SetOwner(ZDOMan.GetSessionID());
                    ZDOMan.instance.DestroyZDO(zdo);
                }
            }
            ZoneSystem.instance.RemoveGlobalKey(WorldKey);
        }

        private static bool TryGetFloorPosition(out Vector3 floor)
        {
            floor = default;
            if (Game.instance == null || ZoneSystem.instance == null ||
                !ZoneSystem.instance.GetLocationIcon(
                    Game.instance.m_StartLocation, out Vector3 temple))
            {
                return false;
            }
            floor = temple + Vector3.forward * HorizontalOffsetFromStartTemple;
            floor.y = HighestNearbyGround(floor) + FloorClearance;
            return true;
        }

        private static float HighestNearbyGround(Vector3 target)
        {
            float highest = float.MinValue;
            for (float x = -GroundScanRadius; x <= GroundScanRadius;
                x += GroundScanStep)
            {
                for (float z = -GroundScanRadius; z <= GroundScanRadius;
                    z += GroundScanStep)
                {
                    Vector3 sample = target + new Vector3(x, 0f, z);
                    highest = Mathf.Max(highest,
                        ZoneSystem.instance.GetGroundHeight(sample));
                }
            }
            return highest;
        }

        private static void SavePosition(Vector3 position)
        {
            string value = string.Join(",",
                position.x.ToString(CultureInfo.InvariantCulture),
                position.y.ToString(CultureInfo.InvariantCulture),
                position.z.ToString(CultureInfo.InvariantCulture));
            ZoneSystem.instance.SetGlobalKey($"{WorldKey} {value}");
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
            return true;
        }

        private static bool IsNetworkPrefab(GameObject prefab)
        {
            return prefab != null && prefab.GetComponent<ZNetView>() != null;
        }
    }
}
