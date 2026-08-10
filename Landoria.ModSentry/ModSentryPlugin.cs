using BepInEx;
using Landoria.SharedLib;

namespace Landoria.ModSentry
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class ModSentryPlugin : LandoriaPlugin
    {
        internal const string InventoryRpc = "Landoria_ModSentry_Inventory";
        internal const string RejectionRpc = "Landoria_ModSentry_Rejection";
        internal const int ProtocolVersion = 1;
        private const string PluginGuid = "Landoria.ModSentry";
        private const string PluginName = "Landoria.ModSentry";
        private const string PluginVersion = "1.0.0";

        internal static ModLog Log { get; private set; }
        internal static PluginPolicy Policy { get; private set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        internal static PluginPolicy EnsurePolicy()
        {
            if (Policy == null)
            {
                Policy = PluginPolicy.Load();
                Log.LogInfo($"Loaded {Policy.Required.Count} required and " +
                            $"{Policy.Optional.Count} optional client mod policies.");
            }

            return Policy;
        }

        private void OnDestroy()
        {
            Log?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            HandshakeState.Clear();
            Policy = null;
            ShutdownPlugin();
            Log = null;
        }
    }
}
