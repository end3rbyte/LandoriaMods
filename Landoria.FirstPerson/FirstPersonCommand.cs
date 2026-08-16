namespace Landoria.FirstPerson
{
    internal static class FirstPersonCommand
    {
        internal static void Register()
        {
            new Terminal.ConsoleCommand("firstperson", string.Empty, Run);
        }

        private static object Run(Terminal.ConsoleEventArgs args)
        {
            bool enabled = !FirstPersonMode.Enabled;
            FirstPersonMode.SetEnabled(enabled);
            FirstPersonPreference.SetEnabled(enabled);
            return true;
        }
    }
}
