namespace Landoria.StructureProtection
{
    internal sealed class StructureProtectionServerConfiguration
    {
        internal bool CreatureTargetingEnabled { get; private set; }
        internal bool WardPlayerDamageEnabled { get; private set; }

        internal static StructureProtectionServerConfiguration FromArguments(string[] arguments)
        {
            return new StructureProtectionServerConfiguration
            {
                CreatureTargetingEnabled = StructureProtectionArgumentPolicy.Resolve(
                    arguments, "--structure-protection-creature-targeting", true),
                WardPlayerDamageEnabled = StructureProtectionArgumentPolicy.Resolve(
                    arguments, "--structure-protection-ward-player-damage", true)
            };
        }
    }
}
