namespace Landoria.HammerFly
{
    internal static class FlyController
    {
        internal static void Toggle()
        {
            Player player = Player.m_localPlayer;
            if (player != null && FlyAuthorization.IsAuthorized)
            {
                SetEnabled(!player.IsDebugFlying());
            }
        }

        internal static void SetEnabled(bool enabled)
        {
            Player player = Player.m_localPlayer;
            if (player == null || enabled && !FlyAuthorization.IsAuthorized)
            {
                return;
            }

            if (player.IsDebugFlying() != enabled)
            {
                player.ToggleDebugFly();
            }
        }

        internal static void OnAuthorizationChanged(bool allowed)
        {
            if (!allowed)
            {
                SetEnabled(false);
            }
        }
    }
}
