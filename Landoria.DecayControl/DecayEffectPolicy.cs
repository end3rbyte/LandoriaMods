namespace Landoria.DecayControl
{
    internal static class DecayEffectPolicy
    {
        internal static bool ShouldApplyEnvironmentalWear(bool isVanillaRainTick,
            bool isPlayerBuilt, DecayControlMode mode, float activityMultiplier)
        {
            if (!isVanillaRainTick || !isPlayerBuilt || mode == DecayControlMode.Default)
            {
                return true;
            }
            return mode == DecayControlMode.PlayerOnline && activityMultiplier > 0f;
        }

        internal static bool ShouldPauseFuel(bool isPlayerBuilt, bool firstUpdate,
            DecayControlMode mode, float activityMultiplier)
        {
            if (!isPlayerBuilt || mode == DecayControlMode.Default)
            {
                return false;
            }
            return mode == DecayControlMode.Disabled || firstUpdate ||
                activityMultiplier <= 0f;
        }
    }
}
