namespace Landoria.HammerFreedom
{
    internal static class FlightGroundActionPolicy
    {
        internal static bool ShouldApply(bool localPlayer, bool flying, bool authorized)
        {
            return !localPlayer || !flying || !authorized;
        }
    }
}
