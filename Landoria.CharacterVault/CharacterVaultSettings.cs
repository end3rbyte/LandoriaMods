using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace Landoria.CharacterVault
{
    internal sealed class CharacterVaultSettings
    {
        private const string ItemsArgument = "--charactervault-starting-items";
        private bool serverInitialized;

        private CharacterVaultSettings(string startingItems)
        {
            StartingItems = ParseItems(startingItems);
        }

        internal bool AllowMultipleCharacters { get; private set; } = true;
        internal IReadOnlyList<StartingItem> StartingItems { get; }

        internal static CharacterVaultSettings Load(ConfigFile config)
        {
            string configuredItems = config.Bind("New Characters", "StartingItems", string.Empty,
                "Comma-separated prefab and quantity pairs, for example hammer:1,wood:10.").Value;
            string items = ReadArgument(ItemsArgument) ?? configuredItems;
            CharacterVaultPlugin.Log.LogInfo($"Effective starting items: '{items}'.");
            return new CharacterVaultSettings(items);
        }

        internal void InitializeServer()
        {
            if (serverInitialized) return;
            AllowMultipleCharacters = CharacterVaultArgumentPolicy.ResolveAllowMultiple(
                Environment.GetCommandLineArgs());
            serverInitialized = true;
            CharacterVaultPlugin.Log.LogInfo(
                $"Server allowMultipleCharacters={AllowMultipleCharacters}.");
        }

        private static string ReadArgument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            string found = null;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (found != null || index + 1 >= arguments.Length)
                {
                    throw new InvalidOperationException($"Command-line switch {name} is missing or duplicated.");
                }

                found = arguments[++index];
            }

            return found;
        }

        private static IReadOnlyList<StartingItem> ParseItems(string value)
        {
            List<StartingItem> items = new List<StartingItem>();
            if (string.IsNullOrWhiteSpace(value))
            {
                return items;
            }

            foreach (string entry in value.Split(','))
            {
                string[] parts = entry.Split(':');
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) ||
                    !int.TryParse(parts[1].Trim(), out int quantity) || quantity <= 0)
                {
                    throw new InvalidOperationException($"Invalid CharacterVault starting item '{entry}'.");
                }

                items.Add(new StartingItem(parts[0].Trim(), quantity));
            }

            return items;
        }
    }

}
