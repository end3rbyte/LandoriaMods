using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.CharacterVault
{
    internal sealed class ServerDisconnectSaveCoordinator : IDisposable
    {
        private const float ConfirmationTimeoutSeconds = 30f;
        private readonly Dictionary<string, PendingServerSave> _pending =
            new Dictionary<string, PendingServerSave>(StringComparer.Ordinal);

        internal bool TryRequest(ZNetPeer peer, string reason,
            Action<string, long, bool> completed, out string requestId)
        {
            requestId = null;
            if (peer?.m_rpc == null || completed == null ||
                ZNet.instance?.IsServer() != true || !peer.IsReady() ||
                CharacterVaultPlugin.Transfers?.CanRequestSave(peer) != true)
            {
                return false;
            }

            requestId = "server-disconnect-" + Guid.NewGuid().ToString("N");
            _pending[requestId] = new PendingServerSave(peer.m_rpc, peer.m_playerName,
                reason, completed);
            CharacterVaultPlugin.Log.LogMessage(
                $"Requesting final save {requestId} for {peer.m_playerName} before {reason}.");
            CharacterVaultPlugin.Transfers.RequestSave(peer, requestId);
            CharacterVaultPlugin.Instance.Run(WaitForConfirmation(requestId));
            return true;
        }

        internal void RecordCommitted(ZRpc rpc, string requestId, long revision)
        {
            if (!_pending.TryGetValue(requestId, out PendingServerSave save) || save.Rpc != rpc)
            {
                return;
            }

            _pending.Remove(requestId);
            CharacterVaultPlugin.Log.LogMessage(
                $"Final save {requestId} for {save.PlayerName} committed at revision {revision}; " +
                $"authorizing {save.Reason}.");
            save.Completed(requestId, revision, true);
        }

        internal void RecordDisconnected(ZRpc rpc)
        {
            foreach (string requestId in RequestsFor(rpc))
            {
                CompleteFailed(requestId, "the connection closed before confirmation");
            }
        }

        public void Dispose()
        {
            foreach (string requestId in new List<string>(_pending.Keys))
            {
                CompleteFailed(requestId, "CharacterVault unloaded before confirmation");
            }
        }

        private IEnumerator WaitForConfirmation(string requestId)
        {
            float deadline = Time.realtimeSinceStartup + ConfirmationTimeoutSeconds;
            while (_pending.ContainsKey(requestId) && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            if (_pending.ContainsKey(requestId))
            {
                CompleteFailed(requestId,
                    $"no commit acknowledgement arrived within {ConfirmationTimeoutSeconds:0} seconds");
            }
        }

        private void CompleteFailed(string requestId, string reason)
        {
            if (!_pending.TryGetValue(requestId, out PendingServerSave save))
            {
                return;
            }

            _pending.Remove(requestId);
            CharacterVaultPlugin.Log.LogError(
                $"Final save {requestId} for {save.PlayerName} failed: {reason}; {save.Reason} is canceled.");
            save.Completed(requestId, 0, false);
        }

        private List<string> RequestsFor(ZRpc rpc)
        {
            List<string> requests = new List<string>();
            foreach (KeyValuePair<string, PendingServerSave> pair in _pending)
            {
                if (pair.Value.Rpc == rpc)
                {
                    requests.Add(pair.Key);
                }
            }

            return requests;
        }
    }

    internal sealed class PendingServerSave
    {
        internal PendingServerSave(ZRpc rpc, string playerName, string reason,
            Action<string, long, bool> completed)
        {
            Rpc = rpc;
            PlayerName = playerName;
            Reason = reason;
            Completed = completed;
        }

        internal Action<string, long, bool> Completed { get; }
        internal string PlayerName { get; }
        internal string Reason { get; }
        internal ZRpc Rpc { get; }
    }
}
