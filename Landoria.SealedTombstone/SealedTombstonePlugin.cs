using BepInEx;
using Landoria.SharedLib;

namespace Landoria.SealedTombstone
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class SealedTombstonePlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.SealedTombstone";
        private const string PluginName = "Landoria.SealedTombstone";
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

        private void Update()
        {
            if (IsEnabled)
            {
                TombstoneAccess.Tick();
            }
        }

        private void OnDestroy()
        {
            TombstoneAccess.ResetSession();
            ShutdownPlugin();
            FeaturePolicy = null;
            Log = null;
        }
    }
}
