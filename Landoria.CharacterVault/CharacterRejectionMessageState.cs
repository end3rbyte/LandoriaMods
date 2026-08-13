namespace Landoria.CharacterVault
{
    internal static class CharacterRejectionMessages
    {
        internal const string PermittedListDenied =
            "Steam account not registered for this server.";
    }

    internal sealed class CharacterRejectionMessageState
    {
        private string _message = string.Empty;

        internal void Receive(string message)
        {
            _message = message;
        }

        internal bool TryGet(out string message)
        {
            message = _message;
            return !string.IsNullOrWhiteSpace(message);
        }

        internal void Clear()
        {
            _message = string.Empty;
        }
    }
}
