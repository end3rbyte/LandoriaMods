using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.Socialize
{
    internal static class GroupStorage
    {
        private const string PrefabName = "LandoriaSocialGroupStorage";
        private const string DataKey = "Landoria.Social.Groups";
        private static ZDO storage;

        internal static bool TryLoad()
        {
            if (storage != null)
            {
                return true;
            }
            storage = FindOrCreate();
            if (storage == null)
            {
                return false;
            }
            Load(storage.GetString(DataKey));
            return true;
        }

        internal static void Save()
        {
            if (storage == null)
            {
                return;
            }
            ZPackage package = new ZPackage();
            package.Write(GroupState.Groups.Count);
            foreach (SocialGroup group in GroupState.Groups.Values)
            {
                WriteGroup(package, group);
            }
            storage.Set(DataKey, Convert.ToBase64String(package.GetArray()));
        }

        internal static void Reset()
        {
            storage = null;
        }

        private static ZDO FindOrCreate()
        {
            if (ZDOMan.instance == null || ZNet.instance == null || !ZNet.instance.IsServer())
            {
                return null;
            }
            List<ZDO> matches = new List<ZDO>();
            int index = 0;
            while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(PrefabName, matches, ref index))
            {
            }
            return matches.Count > 0 ? matches[0] : Create();
        }

        private static ZDO Create()
        {
            int prefabHash = PrefabName.GetStableHashCode();
            Vector3 storagePosition = new Vector3(1000000f, -10000f, 1000000f);
            ZDO zdo = ZDOMan.instance.CreateNewZDO(storagePosition, prefabHash);
            zdo.Persistent = true;
            zdo.Distant = true;
            zdo.SetPrefab(prefabHash);
            SocializePlugin.Log.LogInfo("Created persistent social group storage for the world.");
            return zdo;
        }

        private static void Load(string encoded)
        {
            GroupState.ClearServer();
            if (string.IsNullOrEmpty(encoded))
            {
                return;
            }
            try
            {
                ReadGroups(new ZPackage(Convert.FromBase64String(encoded)));
                SocializePlugin.Log.LogInfo($"Loaded {GroupState.Groups.Count} social group(s).");
            }
            catch (Exception exception)
            {
                SocializePlugin.Log.LogError("Could not load social groups: " + exception);
            }
        }

        private static void ReadGroups(ZPackage package)
        {
            int count = package.ReadInt();
            for (int index = 0; index < count; index++)
            {
                SocialGroup group = ReadGroup(package);
                GroupState.Groups[group.Id] = group;
            }
        }

        private static SocialGroup ReadGroup(ZPackage package)
        {
            SocialGroup group = new SocialGroup { Id = package.ReadInt(), Leader = package.ReadLong() };
            int count = package.ReadInt();
            for (int index = 0; index < count; index++)
            {
                long playerId = package.ReadLong();
                group.Members[playerId] = package.ReadString();
                GroupState.PlayerGroups[playerId] = group.Id;
            }
            return group;
        }

        private static void WriteGroup(ZPackage package, SocialGroup group)
        {
            package.Write(group.Id);
            package.Write(group.Leader);
            package.Write(group.Members.Count);
            foreach (KeyValuePair<long, string> member in group.Members)
            {
                package.Write(member.Key);
                package.Write(member.Value);
            }
        }
    }
}
