using BepInEx;
using BepInEx.Configuration;
using Landoria.SharedLib;

namespace Landoria.HammerFly
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class HammerFlyPlugin : LandoriaPlugin
    {
        internal const string PluginGuid = "Landoria.HammerFly";
        internal const string PluginName = "Landoria.HammerFly";
        internal const string PluginVersion = "1.0.0";

        private ConfigEntry<KeyboardShortcut> _enableShortcut;
        private ConfigEntry<KeyboardShortcut> _disableShortcut;
        internal static ModLog ModLogger { get; private set; }

        private void Awake()
        {
            ModLogger = InitializePlugin(PluginGuid);
            _enableShortcut = Config.Bind("Keyboard", "EnableFly", new KeyboardShortcut(UnityEngine.KeyCode.F6),
                "Enables server-authorized vanilla flight.");
            _disableShortcut = Config.Bind("Keyboard", "DisableFly", new KeyboardShortcut(UnityEngine.KeyCode.F7),
                "Disables vanilla flight.");
            FlyCommand.Register();
            ModLogger.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void Update()
        {
            FlyAuthorization.Update();
            HandleShortcuts();
        }

        private void HandleShortcuts()
        {
            if (!FlyInput.IsAvailable())
            {
                return;
            }

            if (_enableShortcut.Value.IsDown())
            {
                FlyController.SetEnabled(true);
            }
            else if (_disableShortcut.Value.IsDown())
            {
                FlyController.SetEnabled(false);
            }
        }

        private void OnDestroy()
        {
            FlyAuthorization.ResetSession();
            ModLogger?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            ShutdownPlugin();
            ModLogger = null;
        }
    }
}
