namespace Landoria.FirstPerson
{
    internal static class FirstPersonMode
    {
        private static float vanillaMinimumDistance;
        private static bool distanceCaptured;

        internal static bool Enabled { get; private set; }
        internal static bool Active { get; private set; }

        internal static void CaptureVanillaDistance(GameCamera camera)
        {
            vanillaMinimumDistance = camera.m_minDistance;
            distanceCaptured = true;
        }

        internal static void Apply(GameCamera camera)
        {
            if (!camera)
            {
                return;
            }

            camera.m_minDistance = Enabled ? 0f : vanillaMinimumDistance;
        }

        internal static void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            if (!enabled)
            {
                Active = false;
            }
            Apply(GameCamera.instance);
        }

        internal static void SetActive(bool active)
        {
            Active = active;
        }

        internal static void SetFieldOfView(GameCamera camera, float fieldOfView)
        {
            if (camera)
            {
                camera.m_fov = fieldOfView;
            }
        }

        internal static void ResetSession()
        {
            SetEnabled(false);
        }

        internal static void Reset()
        {
            if (distanceCaptured && GameCamera.instance)
            {
                GameCamera.instance.m_minDistance = vanillaMinimumDistance;
            }

            ResetSession();
            Active = false;
            distanceCaptured = false;
        }
    }
}
