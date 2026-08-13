namespace Landoria.CharacterVault
{
    internal interface ICharacterProfileCatalog
    {
        bool HasProfile(string accountId);
    }

    internal enum CharacterAdmission
    {
        ExistingProfile,
        NewEnrollment,
        RejectUnregisteredProfile,
        RejectAdditionalCharacter,
        RejectConcurrentEnrollment
    }

    internal static class CharacterAdmissionPolicy
    {
        internal static CharacterAdmission Decide(bool hasStoredProfile, bool createdThisSession,
            bool allowMultipleCharacters, bool accountHasProfile, bool enrollmentAvailable)
        {
            if (hasStoredProfile)
            {
                return CharacterAdmission.ExistingProfile;
            }
            if (!createdThisSession)
            {
                return CharacterAdmission.RejectUnregisteredProfile;
            }
            if (!allowMultipleCharacters && accountHasProfile)
            {
                return CharacterAdmission.RejectAdditionalCharacter;
            }
            return enrollmentAvailable ? CharacterAdmission.NewEnrollment
                : CharacterAdmission.RejectConcurrentEnrollment;
        }
    }

    internal sealed class CharacterAdmissionEvaluator
    {
        private readonly ICharacterProfileCatalog _profiles;

        internal CharacterAdmissionEvaluator(ICharacterProfileCatalog profiles)
        {
            _profiles = profiles;
        }

        internal CharacterAdmission Decide(bool hasStoredProfile, string accountId,
            bool createdThisSession, bool allowMultipleCharacters, bool enrollmentAvailable)
        {
            bool accountHasProfile = !hasStoredProfile && createdThisSession &&
                !allowMultipleCharacters && _profiles.HasProfile(accountId);
            return CharacterAdmissionPolicy.Decide(hasStoredProfile, createdThisSession,
                allowMultipleCharacters, accountHasProfile, enrollmentAvailable);
        }
    }

    internal sealed class ServerProfileSessionState
    {
        internal bool CanSave => Verified && Admitted && Permitted;
        internal bool PermissionChecked { get; private set; }
        internal bool Verified { get; set; }
        internal bool Admitted { get; set; }
        internal bool Permitted { get; private set; }

        internal void RecordPermission(bool permitted)
        {
            PermissionChecked = true;
            Permitted = permitted;
        }
    }
}
