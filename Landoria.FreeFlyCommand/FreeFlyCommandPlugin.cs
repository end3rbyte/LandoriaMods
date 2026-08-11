using System;
using BepInEx;
using Landoria.SharedLib;

namespace Landoria.FreeFlyCommand
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class FreeFlyCommandPlugin : LandoriaPlugin
    {
        internal const string PluginGuid = "Landoria.FreeFlyCommand";
        internal const string PluginName = "Landoria.FreeFlyCommand";
        internal const string PluginVersion = "1.0.2";
        private const string EnabledArgument = "--freeflycommand";

        internal static ModLog ModLogger { get; private set; }
        internal static bool ServerEnabled { get; private set; } = true;

        private void Awake()
        {
            ModLogger = InitializePlugin(PluginGuid);
            ServerEnabled = ReadServerEnabled();
            FreeFlyCommands.Register();
            ModLogger.LogInfo($"{PluginName} {PluginVersion} is loaded; server enabled={ServerEnabled}.");
        }

        private void Update()
        {
            FreeFlyAuthorization.Update();
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

        private void OnDestroy()
        {
            FreeFlyAuthorization.ResetSession();
            ModLogger?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            ShutdownPlugin();
            ModLogger = null;
        }
    }
}
