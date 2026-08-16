namespace Landoria.FirstPerson
{
    internal static class FirstPersonPolicy
    {
        internal const float DistanceThreshold = 0.001f;

        internal static bool ShouldPersistFieldOfView(
            string command, int argumentCount, bool parsed, float fieldOfView)
        {
            return command == "fov" && argumentCount > 1 && parsed && fieldOfView > 5f;
        }

        internal static bool ShouldResetFieldOfView(
            string command, int argumentCount, string value)
        {
            return command == "fov" && argumentCount == 2 &&
                   string.Equals(value, "reset", System.StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ShouldHidePlayer(
            bool modeEnabled, bool hasLocalPlayer, bool isPlayerDead,
            bool isFreeFly, float cameraDistance)
        {
            return modeEnabled && hasLocalPlayer && !isPlayerDead && !isFreeFly &&
                   cameraDistance <= DistanceThreshold;
        }
    }
}
