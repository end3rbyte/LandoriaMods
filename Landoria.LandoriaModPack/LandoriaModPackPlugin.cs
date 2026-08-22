using BepInEx;
using System.Collections;
using HarmonyLib;

namespace Landoria.LandoriaModPack
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("Landoria.ModSentry", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class LandoriaModPackPlugin : BaseUnityPlugin
    {
        private const string PluginGuid = "Landoria.LandoriaModPack";
        private const string PluginName = "Landoria.LandoriaModPack";
        private const string PluginVersion = "1.0.27";
        private static LandoriaModPackPlugin _instance;
        private Harmony _harmony;

        private void Awake()
        {
            _instance = this;
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
        }

        internal static void Run(IEnumerator routine)
        {
            _instance.StartCoroutine(routine);
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            _instance = null;
        }
    }
}
