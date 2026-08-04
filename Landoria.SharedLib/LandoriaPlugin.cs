using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Landoria.SharedLib
{
    public abstract class LandoriaPlugin : BaseUnityPlugin
    {
        private Harmony _harmony;
        private Harmony _featurePolicyHarmony;
        private ServerFeaturePolicy _featurePolicy;
        private bool _patchesApplied;

        protected ModLog InitializePlugin(string pluginGuid)
        {
            ModLog log = new ModLog(Logger);
            Version assemblyVersion = GetType().Assembly.GetName().Version;
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

        protected ServerFeaturePolicy InitializeServerFeaturePolicy(
            string pluginGuid,
            string pluginVersion,
            ModLog log)
        {
            _featurePolicyHarmony = FeaturePolicyHarmony.Apply(pluginGuid);
            _featurePolicy = new ServerFeaturePolicy(
                Config,
                pluginGuid,
                pluginVersion,
                log,
                Application.isBatchMode);
            return _featurePolicy;
        }

        protected void ShutdownPlugin()
        {
            if (!_patchesApplied)
            {
                return;
            }

            _harmony?.UnpatchSelf();
            _featurePolicy?.Dispose();
            _featurePolicyHarmony?.UnpatchSelf();
            _featurePolicy = null;
            _featurePolicyHarmony = null;
            _patchesApplied = false;
        }
    }
}
