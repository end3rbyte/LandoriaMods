namespace Landoria.Parked
{
    internal static class CreatureProtectionPolicy
    {
        internal static bool CanTarget(bool vanillaCanTarget, float activityMultiplier)
        {
            return vanillaCanTarget && activityMultiplier > 0f;
        }

        internal static bool CanDamageBuilding(float activityMultiplier)
        {
            return activityMultiplier > 0f;
        }
    }
}
