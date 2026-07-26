namespace Landoria.Moderator
{
    internal static class ItemSetCommand
    {
        private const string Usage = "Usage: itemset <meadows|blackforest|swamp|mountain|plains>";

        internal static void Register()
        {
            new Terminal.ConsoleCommand(
                "itemset", "[biome] - Replaces current items with a biome item set.",
                ModeratorCommandAudit.Wrap(Execute),
                isCheat: false, isNetwork: false, onlyServer: false,
                allowInDevBuild: true, onlyAdmin: true);
        }

        private static void Execute(Terminal.ConsoleEventArgs args)
        {
            string itemSet = args.Length == 2 ? GetItemSet(args[1]) : null;
            if (itemSet == null)
            {
                args.Context.AddString(Usage);
                return;
            }

            if (ItemSets.instance == null || !ItemSets.instance.TryGetSet(itemSet, dropCurrentItems: true))
            {
                Fail(args, itemSet);
                return;
            }

            args.Context.AddString($"Applied the {itemSet} item set.");
            ModeratorPlugin.ModLogger.LogDebug($"The {itemSet} item set was applied.");
        }

        private static string GetItemSet(string biome)
        {
            switch (biome.ToLowerInvariant())
            {
                case "meadows": return "Meadows";
                case "blackforest": return "BlackForest";
                case "swamp": return "Swamps";
                case "mountain": return "Mountains";
                case "plains": return "Plains";
                default: return null;
            }
        }

        private static void Fail(Terminal.ConsoleEventArgs args, string itemSet)
        {
            string message = $"The {itemSet} item set could not be applied.";
            args.Context.AddString(message);
            ModeratorPlugin.ModLogger.LogError(message);
        }
    }
}
