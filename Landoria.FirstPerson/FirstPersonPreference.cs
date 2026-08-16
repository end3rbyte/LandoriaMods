using BepInEx.Configuration;

namespace Landoria.FirstPerson
{
    internal static class FirstPersonPreference
    {
        private static ConfigEntry<bool> enabled;
        private static ConfigEntry<float> fieldOfView;

        internal static bool Enabled => enabled.Value;
        internal static float FieldOfView => fieldOfView.Value;

        internal static void Initialize(ConfigFile config)
        {
            enabled = config.Bind(
                "Camera", "FirstPersonEnabled", false,
                "Whether first-person view is enabled at minimum camera zoom.");
            fieldOfView = config.Bind(
                "Camera", "FieldOfView", 65f,
                "Field of view shared by first-person, third-person, and free-fly cameras.");
        }

        internal static void SetEnabled(bool value)
        {
            enabled.Value = value;
        }

        internal static void SetFieldOfView(float value)
        {
            fieldOfView.Value = value;
        }
    }
}
