using Landoria.SharedLib;

namespace Landoria.HammerFreedom
{
    internal sealed class HammerFreedomSettings
    {
        private const string FlightArgument = "--hammerfreedom-fly";
        private const string FallDamageArgument = "--hammerfreedom-fall-damage-immunity";
        private const string StaminaArgument = "--hammerfreedom-unlimited-stamina";

        internal bool Flight { get; private set; } = true;
        internal bool FallDamageImmunity { get; private set; } = true;
        internal bool UnlimitedStamina { get; private set; } = true;

        internal static HammerFreedomSettings FromArguments(string[] arguments, ModLog logger)
        {
            return new HammerFreedomSettings
            {
                Flight = Read(arguments, FlightArgument, logger),
                FallDamageImmunity = Read(arguments, FallDamageArgument, logger),
                UnlimitedStamina = Read(arguments, StaminaArgument, logger)
            };
        }

        private static bool Read(string[] arguments, string name, ModLog logger)
        {
            bool enabled = HammerFreedomArgumentPolicy.Resolve(arguments, name, out bool valid);
            if (!valid)
            {
                logger.LogWarning($"Invalid {name} value; using true.");
            }
            return enabled;
        }
    }
}
