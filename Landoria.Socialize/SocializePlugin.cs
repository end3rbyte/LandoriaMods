using BepInEx;
using Landoria.SharedLib;

namespace Landoria.Socialize
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class SocializePlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.Socialize";
        private const string PluginName = "Landoria.Socialize";
        private const string PluginVersion = "1.0.5";

        internal static ModLog Log { get; private set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void Update()
        {
            GroupService.Update();
            TargetPingService.Update();
        }

        private void OnDestroy()
        {
            GroupService.Reset();
            TargetPingService.Reset();
            Log?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            ShutdownPlugin();
            Log = null;
        }
    }
}
