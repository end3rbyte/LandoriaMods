using Moq;
using Xunit;

namespace Landoria.CharacterVault;

public sealed class CharacterAdmissionEvaluatorMoqTests
{
    [Fact]
    public void ExistingProfileDoesNotQueryOtherAccountProfiles()
    {
        // A direct profile match is admitted without scanning the account's other characters.
        Mock<ICharacterProfileCatalog> profiles = new(MockBehavior.Strict);
        CharacterAdmissionEvaluator evaluator = new(profiles.Object);

        CharacterAdmission admission = evaluator.Decide(hasStoredProfile: true, "Steam_1",
            createdThisSession: false, allowMultipleCharacters: false,
            enrollmentAvailable: false);

        Assert.Equal(CharacterAdmission.ExistingProfile, admission);
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public void UnregisteredOldCharacterDoesNotQueryOtherAccountProfiles()
    {
        // A non-new unknown profile is rejected before checking the multiple-character policy.
        Mock<ICharacterProfileCatalog> profiles = new(MockBehavior.Strict);
        CharacterAdmissionEvaluator evaluator = new(profiles.Object);

        CharacterAdmission admission = evaluator.Decide(hasStoredProfile: false, "Steam_1",
            createdThisSession: false, allowMultipleCharacters: false,
            enrollmentAvailable: true);

        Assert.Equal(CharacterAdmission.RejectUnregisteredProfile, admission);
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public void MultipleCharactersEnabledDoesNotScanExistingProfiles()
    {
        // Enabling multiple characters bypasses the account-level profile lookup.
        Mock<ICharacterProfileCatalog> profiles = new(MockBehavior.Strict);
        CharacterAdmissionEvaluator evaluator = new(profiles.Object);

        CharacterAdmission admission = evaluator.Decide(hasStoredProfile: false, "Steam_1",
            createdThisSession: true, allowMultipleCharacters: true,
            enrollmentAvailable: true);

        Assert.Equal(CharacterAdmission.NewEnrollment, admission);
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public void ExistingAccountProfileIsQueriedOnceAndRejectsSecondCharacter()
    {
        // A single account lookup rejects a second character when multiple characters are disabled.
        Mock<ICharacterProfileCatalog> profiles = new(MockBehavior.Strict);
        profiles.Setup(catalog => catalog.HasProfile("Steam_1")).Returns(true);
        CharacterAdmissionEvaluator evaluator = new(profiles.Object);

        CharacterAdmission admission = evaluator.Decide(hasStoredProfile: false, "Steam_1",
            createdThisSession: true, allowMultipleCharacters: false,
            enrollmentAvailable: true);

        Assert.Equal(CharacterAdmission.RejectAdditionalCharacter, admission);
        profiles.Verify(catalog => catalog.HasProfile("Steam_1"), Times.Once);
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public void EmptyAccountIsQueriedOnceAndAllowsFirstEnrollment()
    {
        // A single negative account lookup permits the first new character enrollment.
        Mock<ICharacterProfileCatalog> profiles = new(MockBehavior.Strict);
        profiles.Setup(catalog => catalog.HasProfile("Steam_1")).Returns(false);
        CharacterAdmissionEvaluator evaluator = new(profiles.Object);

        CharacterAdmission admission = evaluator.Decide(hasStoredProfile: false, "Steam_1",
            createdThisSession: true, allowMultipleCharacters: false,
            enrollmentAvailable: true);

        Assert.Equal(CharacterAdmission.NewEnrollment, admission);
        profiles.Verify(catalog => catalog.HasProfile("Steam_1"), Times.Once);
        profiles.VerifyNoOtherCalls();
    }

    // A new profile rejected by the permitted list remains unsaved and receives the server reason.
    [Fact]
    public void NewProfileWithDeniedSteamIdReceivesPermittedListMessage()
    {
        Mock<ICharacterProfileCatalog> profiles = new(MockBehavior.Strict);
        profiles.Setup(catalog => catalog.HasProfile("Steam_1")).Returns(false);
        CharacterAdmissionEvaluator evaluator = new(profiles.Object);
        ServerProfileSessionState session = new() { Verified = true, Admitted = true };
        CharacterRejectionMessageState clientMessage = new();

        CharacterAdmission admission = evaluator.Decide(hasStoredProfile: false, "Steam_1",
            createdThisSession: true, allowMultipleCharacters: false,
            enrollmentAvailable: true);
        session.RecordPermission(permitted: false);
        clientMessage.Receive(CharacterRejectionMessages.PermittedListDenied);

        Assert.Equal(CharacterAdmission.NewEnrollment, admission);
        Assert.False(session.CanSave);
        Assert.True(clientMessage.TryGet(out string message));
        Assert.Equal("Steam account not registered for this server.", message);
        profiles.Verify(catalog => catalog.HasProfile("Steam_1"), Times.Once);
        profiles.VerifyNoOtherCalls();
    }
}
