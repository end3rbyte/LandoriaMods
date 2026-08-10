using System;
using System.Collections.Generic;

namespace Landoria.FlyCommand
{
    internal static class FlyCommand
    {
        private static Terminal.ConsoleCommand _command;

        internal static void Register()
        {
            _command = new Terminal.ConsoleCommand(
                "fly",
                "[on|off] toggles server-authorized vanilla flight.",
                Run,
                optionsFetcher: Options);
        }

        internal static bool IsCommand(Terminal.ConsoleCommand command)
        {
            return ReferenceEquals(command, _command);
        }

        private static object Run(Terminal.ConsoleEventArgs args)
        {
            if (!FlyAuthorization.IsAuthorized)
            {
                return "Flight is not authorized by this server.";
            }

            if (args.Length == 1)
            {
                FlyController.Toggle();
                return true;
            }

            string value = args[1].ToLowerInvariant();
            if (value != "on" && value != "off")
            {
                return "Use fly, fly on, or fly off.";
            }

            FlyController.SetEnabled(value == "on");
            return true;
        }

        private static List<string> Options()
        {
            return new List<string> { "on", "off" };
        }
    }
}
