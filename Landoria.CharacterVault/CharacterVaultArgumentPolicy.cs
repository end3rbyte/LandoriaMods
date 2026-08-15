using System;

namespace Landoria.CharacterVault
{
    internal static class CharacterVaultArgumentPolicy
    {
        private const string MultipleArgument =
            "--charactervault-allow-multiple-characters";

        internal static bool ResolveAllowMultiple(string[] arguments)
        {
            string value = "";
            bool found = false;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(arguments[index], MultipleArgument,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (found || index + 1 >= arguments.Length)
                {
                    throw new InvalidOperationException(
                        $"Command-line switch {MultipleArgument} is missing or duplicated.");
                }
                value = arguments[++index];
                found = true;
            }
            if (!found) return true;
            if (!bool.TryParse(value, out bool parsed))
            {
                throw new InvalidOperationException(
                    $"Command-line switch {MultipleArgument} requires true or false.");
            }
            return parsed;
        }
    }
}
