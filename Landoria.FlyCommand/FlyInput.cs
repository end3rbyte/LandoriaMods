namespace Landoria.FlyCommand
{
    internal static class FlyInput
    {
        internal static bool IsAvailable()
        {
            return FlyAuthorization.IsAuthorized && Player.m_localPlayer != null &&
                   !Console.IsVisible() && !TextInput.IsVisible() && !InventoryGui.IsVisible() &&
                   !Menu.IsVisible() && (Chat.instance == null || !Chat.instance.HasFocus());
        }
    }
}
