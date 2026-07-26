using UnityEngine;

namespace Landoria.Moderator
{
    internal static class SpawnCommand
    {
        internal static void Register()
        {
            new Terminal.ConsoleCommand("spawn", "[prefab] [amount=1] [level=1] [radius=0.5]",
                ModeratorCommandAudit.Wrap(Execute),
                isCheat: false, isNetwork: false, onlyServer: false,
                allowInDevBuild: true, optionsFetcher: GetPrefabNames,
                onlyAdmin: true);
        }

        private static void Execute(Terminal.ConsoleEventArgs args)
        {
            if (args.Length < 2 || ZNetScene.instance == null || Player.m_localPlayer == null)
            {
                Fail(args, "Usage: spawn <prefab> [amount] [level] [radius]");
                return;
            }

            GameObject prefab = ZNetScene.instance.GetPrefab(args[1]);
            if (prefab == null)
            {
                Fail(args, $"Prefab not found: {args[1]}");
                return;
            }

            Spawn(prefab, args.TryParameterInt(2, 1), args.TryParameterInt(3, 1),
                args.TryParameterFloat(4, 0.5f));
            args.Context.AddString($"Spawned prefab: {args[1]}");
            ModeratorPlugin.ModLogger.LogDebug(
                $"Spawn command created prefab '{args[1]}'.");
        }

        private static void Spawn(GameObject prefab, int amount, int level, float radius)
        {
            amount = Mathf.Max(1, amount);
            for (int index = 0; index < amount; index++)
            {
                Vector3 offset = Random.insideUnitSphere * radius;
                Vector3 position = Player.m_localPlayer.transform.position +
                                   Player.m_localPlayer.transform.forward * 2f + Vector3.up + offset;
                GameObject spawned = Object.Instantiate(prefab, position, Quaternion.identity);
                ItemDrop.OnCreateNew(spawned);
                spawned.GetComponent<Character>()?.SetLevel(Mathf.Clamp(level, 1, 9));
                spawned.GetComponent<ItemDrop>()?.SetQuality(Mathf.Clamp(level, 1, 4));
            }
        }

        private static System.Collections.Generic.List<string> GetPrefabNames()
        {
            return ZNetScene.instance == null
                ? new System.Collections.Generic.List<string>()
                : ZNetScene.instance.GetPrefabNames();
        }

        private static void Fail(Terminal.ConsoleEventArgs args, string message)
        {
            ModeratorPlugin.ModLogger.LogError($"Spawn command failed: {message}");
            args.Context.AddString(message);
        }
    }
}
