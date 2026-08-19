namespace Landoria.ModSentry
{
    internal sealed class ClientRejectionState
    {
        private string _message;
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
            _message = null;
        }
    }
}
