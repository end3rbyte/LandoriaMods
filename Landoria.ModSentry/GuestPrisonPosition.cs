using System.Globalization;

namespace Landoria.ModSentry
{
    internal static class GuestPrisonPosition
    {
        internal static bool TryParse(string value, out float x, out float y, out float z)
        {
            x = 0f;
            y = 0f;
            z = 0f;
            string[] parts = value?.Split(',');
            return parts?.Length == 3 && TryParse(parts[0], out x) &&
                TryParse(parts[1], out y) && TryParse(parts[2], out z);
        }

        private static bool TryParse(string value, out float coordinate)
        {
            return float.TryParse(value?.Trim(), NumberStyles.Float,
                CultureInfo.InvariantCulture, out coordinate) &&
                !float.IsNaN(coordinate) && !float.IsInfinity(coordinate);
        }
    }
}
