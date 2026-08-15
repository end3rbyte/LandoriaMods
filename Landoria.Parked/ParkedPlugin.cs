using BepInEx;
using Landoria.SharedLib;
using Landoria.Socialize;

namespace Landoria.Parked
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("Landoria.Socialize", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class ParkedPlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.Parked";
        private const string PluginName = "Landoria.Parked";
        private const string PluginVersion = "1.0.1";

        internal static ModLog Log { get; private set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            ParkedIntegration.Register(ResetState, WriteState, ReadState);
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private static void ResetState()
        {
            DecayProtection.Reset();
            PiecePermissions.Reset();
        }

        private static void WriteState(ZPackage package)
        {
            DecayProtection.WriteState(package);
            PiecePermissions.WriteState(package);
        }

        private static void ReadState(ZPackage package)
        {
            DecayProtection.ReadState(package);
            PiecePermissions.ReadState(package);
        }

        private void OnDestroy()
        {
            ParkedIntegration.Unregister();
            ResetState();
            Log?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            ShutdownPlugin();
            Log = null;
        }
    }
}
