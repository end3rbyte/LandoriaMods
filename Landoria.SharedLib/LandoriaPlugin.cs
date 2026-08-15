using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace Landoria.SharedLib
{
    public abstract class LandoriaPlugin : BaseUnityPlugin
    {
        private Harmony _harmony;
        private bool _patchesApplied;

        protected ModLog InitializePlugin(string pluginGuid)
        {
            ModLog log = new ModLog(Logger);
            System.Version assemblyVersion = GetType().Assembly.GetName().Version;
            log.LogInfo($"AssemblyVersion: {assemblyVersion}.");
            _harmony = new Harmony(pluginGuid);
            PatchOwnNamespace(log);
            return log;
        }

        protected void PatchOwnNamespace(ModLog log)
        {
            if (_patchesApplied)
            {
                log.LogDebug("Harmony patches are already active; skipping registration.");
                return;
            }

            string pluginNamespace = GetType().Namespace;
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.Namespace == pluginNamespace)
                {
                    _harmony.CreateClassProcessor(type).Patch();
                }
            }

            _patchesApplied = true;
            log.LogDebug("Harmony patches were applied for the plugin namespace.");
        }

        protected void ShutdownPlugin()
        {
            if (!_patchesApplied)
            {
                return;
            }

            _harmony?.UnpatchSelf();
            _patchesApplied = false;
        }
    }
}
