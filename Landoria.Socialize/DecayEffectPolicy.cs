namespace Landoria.Socialize
{
    internal static class DecayEffectPolicy
    {
        internal static bool ShouldApplyRainDamage(
            bool isVanillaRainTick, float activityMultiplier)
        {
            return !isVanillaRainTick || activityMultiplier > 0f;
        }

        internal static bool ShouldPauseFuel(
            bool firstUpdate, float activityMultiplier)
        {
            return firstUpdate || activityMultiplier <= 0f;
        }
    }
}
