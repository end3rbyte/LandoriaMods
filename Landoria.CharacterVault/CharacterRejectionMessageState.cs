namespace Landoria.CharacterVault
{
    internal static class CharacterRejectionMessages
    {
        internal const string PermittedListDenied =
            "Steam account not registered for this server.";
        internal const string AdditionalCharacterDenied =
            "This Steam account already has a character.";
    }

    internal static class PermittedListRejectionPolicy
    {
        internal static string MessageFor(bool isNewCharacter)
        {
            return CharacterRejectionMessages.PermittedListDenied;
        }
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
