using System;
using System.Globalization;
using Landoria.SharedLib;
using UnityEngine;

namespace Landoria.SlowDecay
{
    internal static class SlowDecaySettings
    {
        private const float MinimumMultiplier = 1f;

        internal static float Resolve(float configured, string[] arguments,
            string switchName, ModLog log)
        {
            float multiplier = Mathf.Max(MinimumMultiplier, configured);
            for (int index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(arguments[index], switchName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return ParseOverride(arguments, index, switchName, multiplier, log);
            }

            return multiplier;
        }

        private static float ParseOverride(string[] arguments, int index,
            string switchName, float fallback, ModLog log)
        {
            if (index + 1 < arguments.Length &&
                float.TryParse(arguments[index + 1], NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float value) && value >= MinimumMultiplier)
            {
                log.LogInfo($"Received command-line switch: {switchName} {value:0.###}.");
                return value;
            }

            log.LogWarning($"Invalid {switchName} value; using the BepInEx configuration.");
            return fallback;
        }
    }
}
