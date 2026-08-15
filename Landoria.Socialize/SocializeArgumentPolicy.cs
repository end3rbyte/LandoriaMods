using System;

namespace Landoria.Socialize
{
    internal static class SocializeArgumentPolicy
    {
        internal static bool Resolve(string[] arguments, string name, bool configured)
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
            if (value == null)
            {
                return configured;
            }
            if (!bool.TryParse(value, out bool parsed))
            {
                throw new InvalidOperationException(
                    $"Command-line switch {name} requires true or false.");
            }
            return parsed;
        }
    }
}
