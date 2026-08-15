using BepInEx;
using Landoria.SharedLib;

namespace Landoria.Parked
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class ParkedPlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.Parked";
        private const string PluginName = "Landoria.Parked";
        private const string PluginVersion = "1.0.0";

        internal static ModLog Log { get; private set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void OnDestroy()
        {
            Log?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            ShutdownPlugin();
            Log = null;
        }
    }
}
