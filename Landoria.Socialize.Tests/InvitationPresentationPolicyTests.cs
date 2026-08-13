using Xunit;

namespace Landoria.Socialize;

public sealed class InvitationPresentationPolicyTests
{
    // Verifies the Yes/No invitation popup content and the actions sent by each answer.
    [Fact]
    public void InvitationPopupOffersAcceptAndRejectActions()
    {
        InvitationPresentation presentation = InvitationPresentationPolicy.Build("Alice");

        Assert.Equal("Group invitation", presentation.Title);
        Assert.Equal("Alice invited you to a group.", presentation.Message);
        Assert.Equal("accept", presentation.AcceptAction);
        Assert.Equal("reject", presentation.RejectAction);
    }
}
