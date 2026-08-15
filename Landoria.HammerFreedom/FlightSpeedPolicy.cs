namespace Landoria.HammerFreedom
{
    internal static class FlightSpeedPolicy
    {
        internal const float NormalSpeed = 4f;
        internal const float SprintSpeed = 7f;

        internal static float ResolveMaximumSpeed(bool sprinting)
        {
            return sprinting ? SprintSpeed : NormalSpeed;
        }

        internal static float ResolveScale(float currentSpeed, bool sprinting)
        {
            float maximumSpeed = ResolveMaximumSpeed(sprinting);
            return currentSpeed > maximumSpeed ? maximumSpeed / currentSpeed : 1f;
        }
    }
}
