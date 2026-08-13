using Xunit;

namespace Landoria.CharacterVault;

public sealed class MultipleCharacterAdmissionTests
{
    // Verifies that a second character is rejected with the configured player-facing message.
    [Fact]
    public void SecondCharacterIsRejectedWhenMultipleCharactersAreDisabled()
    {
        CharacterAdmission admission = CharacterAdmissionPolicy.Decide(
            hasStoredProfile: false,
            createdThisSession: true,
            allowMultipleCharacters: false,
            accountHasProfile: true,
            enrollmentAvailable: true);

        Assert.Equal(CharacterAdmission.RejectAdditionalCharacter, admission);
        Assert.Equal(
            "This Steam account already has a character.",
            CharacterAdmissionMessages.ForRejection(admission));
    }

    // Verifies that a second character can enroll when multiple characters are enabled.
    [Fact]
    public void SecondCharacterIsAdmittedWhenMultipleCharactersAreEnabled()
    {
        CharacterAdmission admission = CharacterAdmissionPolicy.Decide(
            hasStoredProfile: false,
            createdThisSession: true,
            allowMultipleCharacters: true,
            accountHasProfile: true,
            enrollmentAvailable: true);

        Assert.Equal(CharacterAdmission.NewEnrollment, admission);
    }
}
