using System;

namespace Landoria.HammerFreedom
{
    internal static class HammerFreedomArgumentPolicy
    {
        internal static bool Resolve(string[] arguments, string name, out bool valid)
        {
            for (int index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                    continue;
                bool enabled = true;
                valid = index + 1 < arguments.Length &&
                    bool.TryParse(arguments[index + 1], out enabled);
                return valid ? enabled : true;
            }

            valid = true;
            return true;
        }
    }
}
