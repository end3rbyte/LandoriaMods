using BepInEx;
using Landoria.SharedLib;

namespace Landoria.GetMyTrophyBack
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class GetMyTrophyBackPlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.GetMyTrophyBack";
        private const string PluginName = "Landoria.GetMyTrophyBack";
        private const string PluginVersion = "1.0.0";

        internal static ModLog Log { get; private set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void OnDestroy()
        {
            ShutdownPlugin();
            Log = null;
        }
    }
}
