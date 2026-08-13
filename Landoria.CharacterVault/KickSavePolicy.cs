namespace Landoria.CharacterVault
{
    internal enum KickSaveEligibility
    {
        Unmanaged,
        Rejected,
        SaveRequired
    }

    internal enum KickAction
    {
        Allow,
        AllowWithoutSave,
        WaitForPendingSave,
        RequestSave,
        Block
    }

    internal static class KickSavePolicy
    {
        internal static KickAction Decide(bool validServerPeer, bool saveAuthorized,
            bool savePending, KickSaveEligibility eligibility)
        {
            if (!validServerPeer || saveAuthorized)
            {
                return KickAction.Allow;
            }
            if (eligibility == KickSaveEligibility.Rejected)
            {
                return KickAction.AllowWithoutSave;
            }
            if (savePending)
            {
                return KickAction.WaitForPendingSave;
            }
            return eligibility == KickSaveEligibility.SaveRequired
                ? KickAction.RequestSave : KickAction.Block;
        }
    }
}
