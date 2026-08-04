using BepInEx;
using Landoria.SharedLib;

namespace Landoria.GentleDeath
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class GentleDeathPlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.GentleDeath";
        private const string PluginName = "Landoria.GentleDeath";
        private const string PluginVersion = "1.0.0";

        internal static ModLog Log { get; private set; }
        internal static bool IsEnabled => FeaturePolicy?.IsEnabled == true;
        private static ServerFeaturePolicy FeaturePolicy { get; set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            FeaturePolicy = InitializeServerFeaturePolicy(PluginGuid, PluginVersion, Log);
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void OnDestroy()
        {
            ShutdownPlugin();
            FeaturePolicy = null;
            Log = null;
        }
    }
}
