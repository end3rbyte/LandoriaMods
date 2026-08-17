namespace Landoria.DecayControl
{
    internal static class DecayEffectPolicy
    {
        internal static bool ShouldApplyEnvironmentalWear(bool isVanillaRainTick,
            bool isPlayerBuilt, DecayControlMode mode, float activityMultiplier)
        {
            if (!isVanillaRainTick || !isPlayerBuilt || mode != DecayControlMode.PlayerOnline)
            {
                return true;
            }
            return activityMultiplier > 0f;
        }

        internal static bool ShouldPauseFuel(bool isPlayerBuilt, bool firstUpdate,
            DecayControlMode mode, float activityMultiplier)
        {
            if (!isPlayerBuilt || mode == DecayControlMode.Default)
            {
                return false;
            }
            return mode == DecayControlMode.PlayerOnline && (firstUpdate ||
                activityMultiplier <= 0f);
        }

        internal static bool ShouldUseNativeInfiniteFuel(bool isPlayerBuilt,
            DecayControlMode mode)
        {
            return isPlayerBuilt && mode == DecayControlMode.Disabled;
        }

        internal static bool ShouldDisableNativeRoofWear(bool isPlayerBuilt,
            DecayControlMode mode)
        {
            return isPlayerBuilt && mode == DecayControlMode.Disabled;
        }
    }
}
