namespace Landoria.ModSentry
{
    internal sealed class ClientRejectionState
    {
        private string _message;
        private float _disconnectDeadline;
        private bool _returnToMenu;

        internal void Receive(string message, float disconnectDeadline)
        {
            _message = message;
            _returnToMenu = true;
            _disconnectDeadline = disconnectDeadline;
        }

        internal bool TryGet(out string message)
        {
            message = _message;
            return !string.IsNullOrWhiteSpace(message);
        }

        internal bool TryBeginReturnToMenu(bool isConnecting, float currentTime)
        {
            if (!_returnToMenu || isConnecting && currentTime < _disconnectDeadline)
            {
                return false;
            }

            _returnToMenu = false;
            return true;
        }

        internal void Clear()
        {
            _message = null;
            _returnToMenu = false;
        }
    }
}
