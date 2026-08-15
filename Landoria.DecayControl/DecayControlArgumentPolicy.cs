using System;

namespace Landoria.DecayControl
{
    internal static class DecayControlArgumentPolicy
    {
        internal static DecayControlMode Resolve(
            string[] arguments, string name, DecayControlMode configured)
        {
            string value = ReadValue(arguments, name);
            if (value == null)
            {
                return configured;
            }
            if (string.Equals(value, "default", StringComparison.OrdinalIgnoreCase))
            {
                return DecayControlMode.Default;
            }
            if (string.Equals(value, "player-online", StringComparison.OrdinalIgnoreCase))
            {
                return DecayControlMode.PlayerOnline;
            }
            if (string.Equals(value, "disabled", StringComparison.OrdinalIgnoreCase))
            {
                return DecayControlMode.Disabled;
            }
            throw new InvalidOperationException(
                $"Command-line switch {name} requires default, player-online, or disabled.");
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
