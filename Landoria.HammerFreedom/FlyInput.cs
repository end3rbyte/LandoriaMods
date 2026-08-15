namespace Landoria.HammerFreedom
{
    internal static class FlyInput
    {
        internal static bool ShouldApplyGroundAction(Character character)
        {
            Player localPlayer = Player.m_localPlayer;
            return FlightGroundActionPolicy.ShouldApply(
                character == localPlayer,
                localPlayer != null && localPlayer.IsDebugFlying(),
                HammerFreedomAuthorization.IsAuthorized(
                    HammerFreedomCapabilities.Flight));
        }

        internal static bool IsAvailable()
        {
            return HammerFreedomAuthorization.IsAuthorized(HammerFreedomCapabilities.Flight) &&
                   Player.m_localPlayer != null &&
                   !Console.IsVisible() && !TextInput.IsVisible() && !InventoryGui.IsVisible() &&
                   !Menu.IsVisible() && (Chat.instance == null || !Chat.instance.HasFocus());
        }
    }
}
