namespace Landoria.FirstPerson
{
    internal static class FirstPersonPolicy
    {
        internal const float DistanceThreshold = 0.001f;
        internal const float MaximumFieldOfView = 85f;
        internal const float FirstPersonFieldOfViewOffset = 15f;

        internal static bool ShouldPersistFieldOfView(
            string command, int argumentCount, bool parsed, float fieldOfView)
        {
            return command == "fov" && argumentCount > 1 && parsed &&
                   fieldOfView > 5f && fieldOfView <= MaximumFieldOfView;
        }

        internal static bool ShouldRejectFieldOfView(
            string command, int argumentCount, bool parsed, float fieldOfView)
        {
            return command == "fov" && argumentCount > 1 && parsed &&
                   fieldOfView > MaximumFieldOfView;
        }

        internal static bool ShouldResetFieldOfView(
            string command, int argumentCount, string value)
        {
            return command == "fov" && argumentCount == 2 &&
                   string.Equals(value, "reset", System.StringComparison.OrdinalIgnoreCase);
        }

        internal static float ClampFieldOfView(float fieldOfView)
        {
            return System.Math.Min(fieldOfView, MaximumFieldOfView);
        }

        internal static float EffectiveFieldOfView(
            float configuredFieldOfView, bool firstPersonActive)
        {
            return configuredFieldOfView +
                   (firstPersonActive ? FirstPersonFieldOfViewOffset : 0f);
        }

        internal static bool ShouldApplyFirstPerson(
            bool modeEnabled, bool hasLocalPlayer, bool isPlayerDead,
            bool isFreeFly, float cameraDistance)
        {
            return modeEnabled && hasLocalPlayer && !isPlayerDead && !isFreeFly &&
                   cameraDistance <= DistanceThreshold;
        }
    }
}
