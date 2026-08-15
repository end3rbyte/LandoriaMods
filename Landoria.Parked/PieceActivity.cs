namespace Landoria.Parked
{
    internal static class PieceActivity
    {
        internal static float GetMultiplier(Piece piece)
        {
            if (piece == null || !piece.IsPlacedByPlayer())
            {
                return 1f;
            }
            long creator = piece.GetCreator();
            if (creator == 0L)
            {
                return 1f;
            }
            return CreatorActivityPolicy.IsCreatorActive(
                creator, ParkedSession.GetOnlinePlayers()) ? 1f : 0f;
        }
    }
}
