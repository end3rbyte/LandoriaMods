namespace Landoria.Moderator
{
    internal static class AdminPlayerModes
    {
        internal static void Apply(bool enabled)
        {
            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }

            player.SetGodMode(enabled);
            player.SetGhostMode(enabled);
            ModeratorPlugin.ModLogger.LogDebug(
                $"Administrator god and ghost modes set to {enabled}.");
        }
    }
}
