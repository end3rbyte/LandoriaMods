namespace Landoria.Socialize
{
    internal sealed class InvitationPresentation
    {
        internal string Title;
        internal string Message;
        internal string AcceptAction;
        internal string RejectAction;
    }

    internal static class InvitationPresentationPolicy
    {
        internal static InvitationPresentation Build(string inviterName)
        {
            return new InvitationPresentation
            {
                Title = "Group invitation",
                Message = inviterName + " invited you to a group.",
                AcceptAction = "accept",
                RejectAction = "reject"
            };
        }
    }
}
