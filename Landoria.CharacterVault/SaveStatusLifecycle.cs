namespace Landoria.CharacterVault
{
    internal sealed class SaveStatusLifecycle
    {
        private string _requestId = string.Empty;
        private bool _waitingForCommit;

        internal int Version { get; private set; }

        internal int Begin(string requestId, bool waitingForCommit)
        {
            Version++;
            _requestId = requestId;
            _waitingForCommit = waitingForCommit;
            return Version;
        }

        internal bool CanCommit(string requestId)
        {
            return _requestId == requestId && _waitingForCommit;
        }

        internal bool CanFail(string requestId, int version)
        {
            return Version == version && _requestId == requestId && _waitingForCommit;
        }

        internal bool IsCurrent(int version)
        {
            return Version == version;
        }

        internal void Clear()
        {
            Version++;
            _requestId = string.Empty;
            _waitingForCommit = false;
        }
    }
}
