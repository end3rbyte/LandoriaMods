using System;
using BepInEx;
using BepInEx.Configuration;
using Landoria.SharedLib;

namespace Landoria.SlowDecay
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class SlowDecayPlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.SlowDecay";
        private const string PluginName = "Landoria.SlowDecay";
        private const string PluginVersion = "1.0.0";
        private const float DefaultSlowdownMultiplier = 10f;
        private const string SlowdownArgument = "--slowdecay";

        internal static float SlowdownMultiplier { get; private set; } = DefaultSlowdownMultiplier;
        internal static ModLog Log { get; private set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            ConfigEntry<float> configured = Config.Bind("General", "SlowdownMultiplier",
                DefaultSlowdownMultiplier,
                "Global divisor for rain damage and fuel consumption. Ten means ten times slower.");
            SlowdownMultiplier = SlowDecaySettings.Resolve(configured.Value,
                Environment.GetCommandLineArgs(), SlowdownArgument, Log);
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded with a {SlowdownMultiplier:0.###}x slowdown.");
        }

        private void OnDestroy()
        {
            Log?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            ShutdownPlugin();
            SlowdownMultiplier = DefaultSlowdownMultiplier;
            Log = null;
        }
    }
}
