namespace Landoria.HammerFreedom
{
    internal static class FlyInput
    {
        internal static bool IsAvailable()
        {
            return HammerFreedomAuthorization.IsAuthorized(HammerFreedomCapabilities.Flight) &&
                   Player.m_localPlayer != null &&
                   !Console.IsVisible() && !TextInput.IsVisible() && !InventoryGui.IsVisible() &&
                   !Menu.IsVisible() && (Chat.instance == null || !Chat.instance.HasFocus());
        }
    }
}
