using BepInEx;
using BepInEx.Configuration;
using Landoria.SharedLib;

namespace Landoria.HammerFreedom
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class HammerFreedomPlugin : LandoriaPlugin
    {
        internal const string PluginGuid = "Landoria.HammerFreedom";
        internal const string PluginName = "Landoria.HammerFreedom";
        internal const string PluginVersion = "1.1.0";
        private static readonly KeyboardShortcut ToggleShortcut =
            new KeyboardShortcut(UnityEngine.KeyCode.Z);

        internal static ModLog ModLogger { get; private set; }
        internal static HammerFreedomSettings Settings { get; private set; }

        private void Awake()
        {
            ModLogger = InitializePlugin(PluginGuid);
            Settings = HammerFreedomSettings.FromArguments(
                System.Environment.GetCommandLineArgs(), ModLogger);
            FlyCommand.Register();
            ModLogger.LogInfo($"{PluginName} {PluginVersion} is loaded; " +
                $"flight={Settings.Flight}, fall damage immunity={Settings.FallDamageImmunity}, " +
                $"unlimited stamina={Settings.UnlimitedStamina}.");
        }

        private void Update()
        {
            HammerFreedomAuthorization.Update();
            HandleShortcuts();
        }

        private void HandleShortcuts()
        {
            if (!FlyInput.IsAvailable())
            {
                return;
            }

            if (ToggleShortcut.IsDown())
            {
                FlyController.Toggle();
            }
        }

        private void OnDestroy()
        {
            HammerFreedomAuthorization.ResetSession();
            ModLogger?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            ShutdownPlugin();
            Settings = null;
            ModLogger = null;
        }
    }
}
