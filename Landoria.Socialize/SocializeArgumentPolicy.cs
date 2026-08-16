using System;
using System.Globalization;

namespace Landoria.Socialize
{
    internal static class SocializeArgumentPolicy
    {
        internal static bool Resolve(string[] arguments, string name, bool configured)
        {
            string value = ReadValue(arguments, name);
            if (value == null) return configured;
            if (!bool.TryParse(value, out bool parsed))
            {
                throw new InvalidOperationException(
                    $"Command-line switch {name} requires true or false.");
            }
            return parsed;
        }

        internal static float ResolvePositiveFloat(
            string[] arguments, string name, float configured)
        {
            string value = ReadValue(arguments, name);
            float parsed = configured;
            bool valid = value == null || float.TryParse(
                value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
            if (!valid || parsed <= 0f || float.IsNaN(parsed) || float.IsInfinity(parsed))
            {
                throw new InvalidOperationException(
                    $"Command-line switch {name} requires a positive finite number.");
            }
            return parsed;
        }

        private static string ReadValue(string[] arguments, string name)
        {
            string value = null;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (value != null || index + 1 >= arguments.Length)
                {
                    throw new InvalidOperationException(
                        $"Command-line switch {name} is missing or duplicated.");
                }
                value = arguments[++index];
            }
            return value;
        }
    }
}
