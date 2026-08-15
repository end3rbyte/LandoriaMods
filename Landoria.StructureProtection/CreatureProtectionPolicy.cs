namespace Landoria.StructureProtection
{
    internal static class CreatureProtectionPolicy
    {
        internal static bool CanTarget(
            bool enabled, bool vanillaCanTarget, float activityMultiplier)
        {
            return vanillaCanTarget && (!enabled || activityMultiplier > 0f);
        }
    }
}
