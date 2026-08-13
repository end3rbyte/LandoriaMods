namespace Landoria.Socialize
{
    internal static class PieceInteractionPolicy
    {
        internal static bool CanUse(bool hasAccess)
        {
            return hasAccess;
        }

        internal static bool CanRemove(bool hasAccess, ref bool result)
        {
            if (hasAccess)
            {
                return true;
            }

            result = false;
            return false;
        }
    }
}
