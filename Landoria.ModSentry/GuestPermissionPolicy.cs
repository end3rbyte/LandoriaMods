namespace Landoria.ModSentry
{
    internal static class GuestPermissionPolicy
    {
        internal static bool Resolve(bool vanillaAllowed, bool guest, bool banned)
        {
            return guest ? !banned : vanillaAllowed;
        }
    }
}
