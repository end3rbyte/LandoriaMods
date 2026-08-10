using System;
using BepInEx;
using BepInEx.Configuration;
using Landoria.SharedLib;

namespace Landoria.FlyCommand
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class FlyCommandPlugin : LandoriaPlugin
    {
        internal const string PluginGuid = "Landoria.FlyCommand";
        internal const string PluginName = "Landoria.FlyCommand";
        internal const string PluginVersion = "1.0.1";
        private const string EnabledArgument = "--flycommand";
        private static readonly KeyboardShortcut ToggleShortcut =
            new KeyboardShortcut(UnityEngine.KeyCode.Z);

        internal static ModLog ModLogger { get; private set; }
        internal static bool ServerEnabled { get; private set; } = true;

        private void Awake()
        {
            ModLogger = InitializePlugin(PluginGuid);
            ServerEnabled = ReadServerEnabled();
            FlyCommand.Register();
            ModLogger.LogInfo($"{PluginName} {PluginVersion} is loaded; server enabled={ServerEnabled}.");
        }

        private static bool ReadServerEnabled()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(arguments[index], EnabledArgument,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (index + 1 < arguments.Length &&
                    bool.TryParse(arguments[index + 1], out bool enabled))
                {
                    return enabled;
                }

                ModLogger.LogWarning($"Invalid {EnabledArgument} value; using true.");
                return true;
            }

            return true;
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

            if (ToggleShortcut.IsDown())
            {
                FlyController.Toggle();
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
