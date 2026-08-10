namespace Landoria.ModSentry
{
    internal sealed class ValidationResult
    {
        private ValidationResult(bool accepted, string playerMessage, string technicalMessage)
        {
            Accepted = accepted;
            PlayerMessage = playerMessage;
            TechnicalMessage = technicalMessage;
        }

        internal bool Accepted { get; }
        internal string PlayerMessage { get; }
        internal string TechnicalMessage { get; }

        internal static ValidationResult Accept()
        {
            return new ValidationResult(true, string.Empty, "Client plugin inventory accepted.");
        }

        internal static ValidationResult Reject(string playerMessage, string technicalMessage)
        {
            return new ValidationResult(false, playerMessage, technicalMessage);
        }
    }
}
