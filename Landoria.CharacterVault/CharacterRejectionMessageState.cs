namespace Landoria.CharacterVault
{
    internal static class CharacterRejectionMessages
    {
        internal const string PermittedListDenied =
            "This platform account is not allowed to join this server.";
        internal const string AdditionalCharacterDenied =
            "This platform account already has a character.";
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
