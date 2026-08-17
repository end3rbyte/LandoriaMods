namespace Landoria.ModSentry
{
    internal static class GuestPermissionPolicy
    {
        internal static bool Resolve(bool vanillaAllowed, bool temporaryGuest, bool banned)
        {
            return temporaryGuest ? !banned : vanillaAllowed;
        }
    }
}
