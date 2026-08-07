using BepInEx;
using Landoria.SharedLib;
using System.Collections.Generic;

namespace Landoria.Moderator
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class ModeratorPlugin : LandoriaPlugin
    {
        private static readonly HashSet<string> CommandsRequiringModerator =
            new HashSet<string>
            {
                "exploremap", "goto", "itemset", "playerlist", "summon",
                "resetmap", "spawn"
            };

        internal static ModLog ModLogger { get; private set; }
        internal static bool IsEnabled => FeaturePolicy?.IsEnabled == true;
        private static ServerFeaturePolicy FeaturePolicy { get; set; }
        private const string PluginGuid = "Landoria.Moderator";
        private const string PluginName = "Landoria.Moderator";
        private const string PluginVersion = "1.0.1";

        private void Awake()
        {
            ModLogger = InitializePlugin(PluginGuid);
            FeaturePolicy = InitializeServerFeaturePolicy(PluginGuid, PluginVersion, ModLogger);
            RegisterCommands();
            ModLogger.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void OnDestroy()
        {
            ModeratorMapSharing.Disable();
            ShutdownPlugin();
            FeaturePolicy = null;
            ModLogger = null;
        }

        private void Update()
        {
            if (IsEnabled)
            {
                PlayerPositionRpc.Update();
                ModeratorMapSharing.Update();
            }
            else if (ModeratorState.IsActive)
            {
                ModeratorState.SetEnabled(false);
            }
        }

        internal static void RegisterCommands()
        {
            ModeratorModeCommand.Register();
            ExploreMapCommand.Register();
            GotoCommand.Register();
            ItemSetCommand.Register();
            PlayerListCommand.Register();
            SummonCommand.Register();
            ResetMapCommand.Register();
            SpawnCommand.Register();
        }

        internal static bool RequiresEnabledModerator(string command)
        {
            return CommandsRequiringModerator.Contains(command);
        }
    }
}
