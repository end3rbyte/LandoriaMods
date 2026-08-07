using BepInEx;
using Landoria.SharedLib;

namespace Landoria.Socialize
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class SocializePlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.Socialize";
        private const string PluginName = "Landoria.Socialize";
        private const string PluginVersion = "1.0.2";

        internal static ModLog Log { get; private set; }
        internal static bool IsEnabled => FeaturePolicy?.IsEnabled == true;
        private static ServerFeaturePolicy FeaturePolicy { get; set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            FeaturePolicy = InitializeServerFeaturePolicy(PluginGuid, PluginVersion, Log);
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void Update()
        {
            if (IsEnabled)
            {
                GroupService.Update();
                TargetPingService.Update();
            }
        }

        private void OnDestroy()
        {
            GroupService.Reset();
            TargetPingService.Reset();
            ShutdownPlugin();
            FeaturePolicy = null;
            Log = null;
        }
    }
}
