using System;
using System.Collections.Generic;
using System.Globalization;

namespace Landoria.FreeFlyCommand
{
    internal static class FreeFlyCommands
    {
        private static Terminal.ConsoleCommand _freeFlyCommand;
        private static Terminal.ConsoleCommand _smoothCommand;

        internal static void Register()
        {
            _freeFlyCommand = new Terminal.ConsoleCommand(
                "freefly",
                "Toggles the server-authorized native free camera.",
                ToggleFreeFly);
            _smoothCommand = new Terminal.ConsoleCommand(
                "ffsmooth",
                "[0-1] sets native free-camera smoothing.",
                SetSmoothness,
                optionsFetcher: SmoothnessOptions);
        }

        internal static bool IsManaged(Terminal.ConsoleCommand command)
        {
            return ReferenceEquals(command, _freeFlyCommand) ||
                   ReferenceEquals(command, _smoothCommand);
        }

        private static object ToggleFreeFly(Terminal.ConsoleEventArgs args)
        {
            if (!FreeFlyAuthorization.IsAuthorized)
            {
                return "Free camera is not authorized by this server.";
            }

            FreeFlyController.Toggle();
            return true;
        }

        private static object SetSmoothness(Terminal.ConsoleEventArgs args)
        {
            if (!FreeFlyAuthorization.IsAuthorized)
            {
                return "Free camera smoothing is not authorized by this server.";
            }

            if (args.Length != 2 ||
                !float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                    out float smoothness) ||
                smoothness < 0f || smoothness > 1f)
            {
                return "Use ffsmooth followed by a value from 0 to 1.";
            }

            GameCamera.instance.SetFreeFlySmoothness(smoothness);
            return true;
        }

        private static List<string> SmoothnessOptions()
        {
            return new List<string> { "0", "0.25", "0.5", "0.75", "1" };
        }
    }
}
