using Landoria.SharedLib;

namespace Landoria.StructureProtection
{
    internal sealed class StructureProtectionSettings
    {
        private bool serverInitialized;

        internal bool CreatureTargetingEnabled { get; private set; }
        internal bool WardPlayerDamageEnabled { get; private set; }

        internal void InitializeServer(ModLog logger)
        {
            if (serverInitialized || !ServerRole.IsDedicatedServer)
            {
                return;
            }
            StructureProtectionServerConfiguration configuration =
                StructureProtectionServerConfiguration.FromArguments(
                    System.Environment.GetCommandLineArgs());
            CreatureTargetingEnabled = configuration.CreatureTargetingEnabled;
            WardPlayerDamageEnabled = configuration.WardPlayerDamageEnabled;
            serverInitialized = true;
            logger.LogInfo($"Effective structure protection settings: " +
                $"creatureTargeting={CreatureTargetingEnabled}, " +
                $"wardPlayerDamage={WardPlayerDamageEnabled}.");
        }

        internal void WriteClientState(ZPackage package)
        {
            package.Write(CreatureTargetingEnabled);
        }

        internal void ReadClientState(ZPackage package)
        {
            CreatureTargetingEnabled = package.ReadBool();
        }

        internal void ResetClientState()
        {
            if (!serverInitialized)
            {
                CreatureTargetingEnabled = false;
                WardPlayerDamageEnabled = false;
            }
        }
    }
}
