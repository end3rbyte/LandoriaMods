using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Landoria.CharacterVault
{
    internal sealed class ProfileTransferService : IDisposable
    {
        internal const string HelloRpc = "CharacterVault_Hello_v1";
        internal const string AdmissionRpc = "CharacterVault_Admission_v1";
        internal const string DownloadBeginRpc = "CharacterVault_DownloadBegin_v1";
        internal const string DownloadChunkRpc = "CharacterVault_DownloadChunk_v1";
        internal const string DownloadCompleteRpc = "CharacterVault_DownloadComplete_v1";
        internal const string UploadBeginRpc = "CharacterVault_UploadBegin_v1";
        internal const string UploadChunkRpc = "CharacterVault_UploadChunk_v1";
        internal const string UploadCompleteRpc = "CharacterVault_UploadComplete_v1";
        internal const string SaveRequestRpc = "CharacterVault_SaveRequest_v1";
        internal const string SaveAckRpc = "CharacterVault_SaveAck_v1";
        private const int ProtocolVersion = 1;
        private const int ChunkSize = 65536;
        private const int MaximumProfileBytes = 64 * 1024 * 1024;
        private readonly Dictionary<ZRpc, VaultSession> _sessions = new Dictionary<ZRpc, VaultSession>();
        private readonly Dictionary<ZRpc, IncomingTransfer> _uploads = new Dictionary<ZRpc, IncomingTransfer>();
        private readonly Dictionary<string, ZRpc> _enrollments = new Dictionary<string, ZRpc>(StringComparer.Ordinal);
        private readonly SynchronizationContext _unityContext;
        private readonly VaultStorage _storage = new VaultStorage();
        private IncomingTransfer _download;
        private bool _clientActive;
        private bool _clientEnrolling;
        private bool _clientUploadBusy;
        private bool _suppressNextClientUpload;
        private string _pendingRequest;
        private PlayerProfile _pendingProfile;
        private IReadOnlyList<StartingItem> _serverStartingItems = Array.Empty<StartingItem>();

        internal ProfileTransferService(SynchronizationContext unityContext)
        {
            _unityContext = unityContext ?? throw new ArgumentNullException(nameof(unityContext));
        }

        internal void Register(ZNet network, ZNetPeer peer)
        {
            if (network.IsServer())
            {
                RegisterServer(peer.m_rpc);
            }
            else
            {
                RegisterClient(peer.m_rpc);
            }
        }

        internal void SendHello(ZRpc serverRpc)
        {
            PlayerProfile profile = Game.instance?.GetPlayerProfile();
            if (profile == null)
            {
                return;
            }

            ZPackage package = new ZPackage();
            package.Write(ProtocolVersion);
            package.Write(profile.GetPlayerID());
            package.Write(profile.GetName());
            package.Write(NewCharacterTracker.WasCreatedThisSession(profile.GetPlayerID()));
            serverRpc.Invoke(HelloRpc, package);
        }

        internal bool Approve(ZRpc rpc)
        {
            if (!_sessions.TryGetValue(rpc, out VaultSession session))
            {
                Reject(rpc, "Character verification did not complete. Please try again.");
                return false;
            }

            if (session.Verified)
            {
                return session.Admitted;
            }

            session.Verified = true;
            if (_storage.TryRead(session.AccountId, session.CharacterId, session.Name,
                out byte[] data, out long revision))
            {
                session.Revision = revision;
                SendDownload(rpc, session, data);
                session.Admitted = true;
                return true;
            }

            session.Admitted = AdmitEnrollment(rpc, session, session.NewCharacter);
            return session.Admitted;
        }

        internal void Remove(ZNetPeer peer)
        {
            if (peer?.m_rpc == null)
            {
                return;
            }

            _sessions.Remove(peer.m_rpc);
            _uploads.Remove(peer.m_rpc);
            ReleaseEnrollment(peer.m_rpc);
            CharacterVaultPlugin.ServerDisconnects?.RecordDisconnected(peer.m_rpc);
            if (ZNet.instance?.IsServer() == false)
            {
                ResetClientState();
            }
        }

        internal void RequestWorldCheckpoint()
        {
            if (ZNet.instance?.IsServer() != true)
            {
                return;
            }

            string request = "world-" + Guid.NewGuid().ToString("N");
            foreach (ZNetPeer peer in ZNet.instance.GetPeers().Where(IsReady))
            {
                RequestSave(peer, request);
            }
        }

        internal void RequestSave(ZNetPeer peer, string requestId)
        {
            if (peer?.m_rpc != null && _sessions.ContainsKey(peer.m_rpc))
            {
                peer.m_rpc.Invoke(SaveRequestRpc, requestId);
            }
        }

        internal bool CanRequestSave(ZNetPeer peer)
        {
            return peer?.m_rpc != null && _sessions.ContainsKey(peer.m_rpc);
        }

        internal void UploadSavedProfile(PlayerProfile profile)
        {
            if (_suppressNextClientUpload)
            {
                _suppressNextClientUpload = false;
                CharacterVaultPlugin.Log.LogInfo(
                    "Skipped the redundant local save upload after a confirmed voluntary disconnect save.");
                return;
            }

            if (!_clientActive || ZNet.instance?.IsServer() != false)
            {
                return;
            }

            ZRpc serverRpc = ZNet.instance.GetServerRPC();
            if (serverRpc == null)
            {
                ResetClientState();
                return;
            }

            string request = _pendingRequest ?? "save-" + Guid.NewGuid().ToString("N");
            _pendingRequest = null;
            if (_clientUploadBusy)
            {
                _pendingRequest = request;
                CharacterVaultPlugin.Log.LogInfo(
                    $"Queued character save request {request} while another upload is awaiting confirmation.");
                return;
            }

            byte[] data = ProfileFile.Read(profile);
            _clientUploadBusy = true;
            CharacterVaultPlugin.Log.LogInfo(
                $"Uploading character profile {profile.GetName()} for save request {request}.");
            CharacterVaultPlugin.Instance.Run(SendUpload(serverRpc, profile, data, request));
        }

        internal bool BeginFinalDisconnectSave(string requestId)
        {
            PlayerProfile profile = Game.instance?.GetPlayerProfile();
            if (!_clientActive || ZNet.instance?.IsServer() != false || profile == null)
            {
                return false;
            }

            _pendingRequest = requestId;
            if (_clientUploadBusy)
            {
                CharacterVaultPlugin.Log.LogInfo(
                    $"Final save request {requestId} is waiting for the active upload to finish.");
                return true;
            }

            CharacterVaultPlugin.Log.LogInfo(
                $"Writing the final local profile for {profile.GetName()} before disconnect.");
            Game.instance.SavePlayerProfile(true);
            return true;
        }

        internal void SuppressRedundantDisconnectUpload()
        {
            _suppressNextClientUpload = true;
        }

        internal void GrantStartingItems()
        {
            if (!_clientEnrolling || Player.m_localPlayer == null)
            {
                return;
            }

            _clientEnrolling = false;
            foreach (StartingItem item in _serverStartingItems)
            {
                GameObject prefab = FindItem(item.Prefab);
                if (prefab == null || !Player.m_localPlayer.GetInventory().AddItem(prefab, item.Quantity))
                {
                    CharacterVaultPlugin.Log.LogError($"Could not grant starting item {item.Prefab}:{item.Quantity}.");
                }
            }

            Game.instance.SavePlayerProfile(true);
        }

        internal void ApplyPendingProfile(ref PlayerProfile profile)
        {
            if (_pendingProfile == null)
            {
                return;
            }

            profile = _pendingProfile;
            _pendingProfile = null;
        }

        public void Dispose()
        {
            _sessions.Clear();
            _uploads.Clear();
            _enrollments.Clear();
            _download = null;
        }

        private void RegisterServer(ZRpc rpc)
        {
            CharacterVaultRejection.RegisterServer(rpc);
            rpc.Register<ZPackage>(HelloRpc, ReceiveHello);
            rpc.Register<ZPackage>(UploadBeginRpc, ReceiveUploadBegin);
            rpc.Register<ZPackage>(UploadChunkRpc, ReceiveUploadChunk);
            rpc.Register<ZPackage>(UploadCompleteRpc, ReceiveUploadComplete);
        }

        private void RegisterClient(ZRpc rpc)
        {
            ResetClientState();
            CharacterVaultPlugin.DisconnectCoordinator?.RecordConnectionStarted();
            CharacterVaultRejection.RegisterClient(rpc);
            rpc.Register<ZPackage>(AdmissionRpc, ReceiveAdmission);
            rpc.Register<ZPackage>(DownloadBeginRpc, ReceiveDownloadBegin);
            rpc.Register<ZPackage>(DownloadChunkRpc, ReceiveDownloadChunk);
            rpc.Register<ZPackage>(DownloadCompleteRpc, ReceiveDownloadComplete);
            rpc.Register<string>(SaveRequestRpc, ReceiveSaveRequest);
            rpc.Register<string, long>(SaveAckRpc, ReceiveSaveAck);
        }

        private void ReceiveHello(ZRpc rpc, ZPackage package)
        {
            if (package.ReadInt() != ProtocolVersion)
            {
                Reject(rpc, "The CharacterVault protocol is incompatible.");
                return;
            }

            long characterId = package.ReadLong();
            string name = package.ReadString();
            bool newCharacter = package.ReadBool();
            string accountId = NormalizeAccount(rpc.GetSocket().GetHostName());
            _sessions[rpc] = new VaultSession(accountId, characterId, name, newCharacter);
        }

        private bool AdmitEnrollment(ZRpc rpc, VaultSession session, bool newCharacter)
        {
            if (!newCharacter || !_storage.CanEnroll(session.AccountId, session.Name,
                CharacterVaultPlugin.Settings.AllowMultipleCharacters) || !ReserveEnrollment(rpc, session))
            {
                _sessions.Remove(rpc);
                Reject(rpc, newCharacter ? "This Steam account already has a character."
                    : "Create a new character before joining this server.");
                return false;
            }

            session.Enrolling = true;
            ZPackage response = new ZPackage();
            response.Write(session.CharacterId);
            response.Write(CharacterVaultPlugin.Settings.StartingItems.Count);
            foreach (StartingItem item in CharacterVaultPlugin.Settings.StartingItems)
            {
                response.Write(item.Prefab);
                response.Write(item.Quantity);
            }
            rpc.Invoke(AdmissionRpc, response);
            return true;
        }

        private void SendDownload(ZRpc rpc, VaultSession session, byte[] data)
        {
            string transferId = Guid.NewGuid().ToString("N");
            string hash = VaultStorage.Hash(data);
            rpc.Invoke(DownloadBeginRpc, BeginPackage(transferId, data.Length, hash));
            for (int offset = 0; offset < data.Length; offset += ChunkSize)
            {
                rpc.Invoke(DownloadChunkRpc, ChunkPackage(transferId, data, offset));
            }

            ZPackage complete = new ZPackage();
            complete.Write(transferId);
            complete.Write(session.Revision);
            rpc.Invoke(DownloadCompleteRpc, complete);
        }

        private void ReceiveAdmission(ZRpc rpc, ZPackage package)
        {
            long characterId = package.ReadLong();
            if (Game.instance.GetPlayerProfile().GetPlayerID() != characterId)
            {
                throw new InvalidDataException("The server admitted a different character.");
            }

            int count = package.ReadInt();
            List<StartingItem> items = new List<StartingItem>(count);
            for (int index = 0; index < count; index++)
            {
                items.Add(new StartingItem(package.ReadString(), package.ReadInt()));
            }

            _serverStartingItems = items;
            _clientActive = true;
            _clientEnrolling = true;
        }

        private void ReceiveDownloadBegin(ZRpc rpc, ZPackage package)
        {
            _download = IncomingTransfer.Create(package, MaximumProfileBytes);
        }

        private void ReceiveDownloadChunk(ZRpc rpc, ZPackage package)
        {
            _download?.Add(package);
        }

        private void ReceiveDownloadComplete(ZRpc rpc, ZPackage package)
        {
            string transferId = package.ReadString();
            package.ReadLong();
            byte[] data = _download?.Complete(transferId);
            _download = null;
            if (data == null)
            {
                throw new InvalidDataException("The authoritative profile transfer was incomplete.");
            }

            _pendingProfile = ProfileFile.ReplaceSelected(data);
            _clientActive = true;
        }

        private void ReceiveSaveRequest(ZRpc rpc, string requestId)
        {
            if (!_clientActive || string.IsNullOrWhiteSpace(requestId))
            {
                return;
            }

            _pendingRequest = requestId;
            if (!_clientUploadBusy)
            {
                Game.instance.SavePlayerProfile(true);
            }
        }

        private void ReceiveSaveAck(ZRpc rpc, string requestId, long revision)
        {
            _clientUploadBusy = false;
            CharacterVaultPlugin.Log.LogInfo(
                $"Server accepted character save request {requestId} at revision {revision}.");
            CharacterVaultPlugin.DisconnectCoordinator?.RecordSaveCommitted(requestId, revision);
            if (_pendingRequest != null)
            {
                Game.instance.SavePlayerProfile(true);
            }
        }

        private void ResetClientState()
        {
            _clientActive = false;
            _clientEnrolling = false;
            _clientUploadBusy = false;
            _suppressNextClientUpload = false;
            _pendingRequest = null;
            _pendingProfile = null;
            CharacterVaultPlugin.DisconnectCoordinator?.RecordConnectionLost();
        }

        private IEnumerator SendUpload(
            ZRpc rpc, PlayerProfile profile, byte[] data, string requestId)
        {
            string transferId = Guid.NewGuid().ToString("N");
            bool sent = false;
            try
            {
                ZPackage begin = BeginPackage(transferId, data.Length, VaultStorage.Hash(data));
                begin.Write(requestId);
                begin.Write(profile.GetPlayerID());
                rpc.Invoke(UploadBeginRpc, begin);
                for (int offset = 0; offset < data.Length; offset += ChunkSize)
                {
                    rpc.Invoke(UploadChunkRpc, ChunkPackage(transferId, data, offset));
                    yield return null;
                }

                ZPackage complete = new ZPackage();
                complete.Write(transferId);
                rpc.Invoke(UploadCompleteRpc, complete);
                sent = true;
            }
            finally
            {
                if (!sent && ZNet.instance?.GetServerRPC() == rpc)
                {
                    _clientUploadBusy = false;
                    CharacterVaultPlugin.Log.LogWarning(
                        $"Character save upload {requestId} was interrupted before completion.");
                }
            }
        }

        private void ReceiveUploadBegin(ZRpc rpc, ZPackage package)
        {
            if (!TryGetVerifiedSession(rpc, out VaultSession session))
            {
                return;
            }

            IncomingTransfer transfer = IncomingTransfer.Create(package, MaximumProfileBytes);
            transfer.RequestId = package.ReadString();
            long characterId = package.ReadLong();
            if (characterId != session.CharacterId)
            {
                throw new InvalidDataException("A peer attempted to save a different character.");
            }

            _uploads[rpc] = transfer;
        }

        private void ReceiveUploadChunk(ZRpc rpc, ZPackage package)
        {
            if (_uploads.TryGetValue(rpc, out IncomingTransfer transfer))
            {
                transfer.Add(package);
            }
        }

        private void ReceiveUploadComplete(ZRpc rpc, ZPackage package)
        {
            string transferId = package.ReadString();
            if (!_uploads.TryGetValue(rpc, out IncomingTransfer transfer) ||
                !TryGetVerifiedSession(rpc, out VaultSession session))
            {
                return;
            }

            _uploads.Remove(rpc);
            byte[] data = transfer.Complete(transferId);
            ValidateProfile(session, data);
            long revision = session.Revision + 1;
            if (session.Enrolling)
            {
                _storage.Commit(session.AccountId, session.Name, data);
                ConfirmCommit(rpc, session, transfer.RequestId, revision);
                return;
            }

            bool voluntaryDisconnect = transfer.RequestId.StartsWith(
                "disconnect-", StringComparison.Ordinal);
            if (voluntaryDisconnect)
            {
                ConfirmReceipt(rpc, session, transfer.RequestId, revision);
            }
            ThreadPool.QueueUserWorkItem(_ => Commit(rpc, session, transfer.RequestId,
                data, revision, voluntaryDisconnect));
        }

        private void ConfirmReceipt(ZRpc rpc, VaultSession session, string requestId, long revision)
        {
            session.Revision = revision;
            rpc.Invoke(SaveAckRpc, requestId, revision);
            CharacterVaultPlugin.Log.LogMessage(
                $"Accepted character profile for {session.Name} at revision {revision} " +
                $"for request {requestId}; durable commit queued.");
        }

        private void Commit(ZRpc rpc, VaultSession session, string requestId, byte[] data,
            long revision, bool receiptConfirmed)
        {
            try
            {
                _storage.Commit(session.AccountId, session.Name, data);
                _unityContext.Post(_ =>
                {
                    if (receiptConfirmed)
                    {
                        ConfirmBackgroundCommit(rpc, session, requestId, revision);
                    }
                    else
                    {
                        ConfirmCommit(rpc, session, requestId, revision);
                    }
                }, null);
            }
            catch (Exception exception)
            {
                CharacterVaultPlugin.Log.LogError($"Character vault commit failed: {exception}");
            }
        }

        private void ConfirmBackgroundCommit(
            ZRpc rpc, VaultSession session, string requestId, long revision)
        {
            CharacterVaultPlugin.Log.LogMessage(
                $"Committed character profile for {session.Name} at revision {revision} " +
                $"for request {requestId}.");
            if (!_sessions.TryGetValue(rpc, out VaultSession current) || current != session)
            {
                return;
            }
            CharacterVaultPlugin.Coordinator?.RecordSaveCommitted(rpc, requestId);
            CharacterVaultPlugin.ServerDisconnects?.RecordCommitted(rpc, requestId, revision);
        }

        private void ConfirmCommit(ZRpc rpc, VaultSession session, string requestId, long revision)
        {
            if (!_sessions.TryGetValue(rpc, out VaultSession current) || current != session)
            {
                return;
            }

            session.Revision = revision;
            session.Enrolling = false;
            ReleaseEnrollment(rpc);
            rpc.Invoke(SaveAckRpc, requestId, revision);
            CharacterVaultPlugin.Log.LogMessage(
                $"Saved character profile for {session.Name} at revision {revision} " +
                $"for request {requestId}.");
            CharacterVaultPlugin.Coordinator?.RecordSaveCommitted(rpc, requestId);
            CharacterVaultPlugin.ServerDisconnects?.RecordCommitted(rpc, requestId, revision);
        }

        private static void ValidateProfile(VaultSession session, byte[] data)
        {
            string filename = "character_vault_validation_" + Guid.NewGuid().ToString("N");
            FileHelpers.FileSource source = SaveApiCompatibility.LocalSource;
            string path = SaveApiCompatibility.GetCharacterPath(source, filename);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, data);
            try
            {
                PlayerProfile profile = new PlayerProfile(filename, source);
                if (!profile.Load() || profile.GetPlayerID() != session.CharacterId ||
                    !string.Equals(profile.GetName(), session.Name, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The uploaded profile identity is invalid.");
                }
            }
            finally
            {
                File.Delete(path);
                SaveApiCompatibility.InvalidateCharacterCache();
            }
        }

        private bool TryGetVerifiedSession(ZRpc rpc, out VaultSession session)
        {
            session = null;
            return _sessions.TryGetValue(rpc, out session) && session.Verified;
        }

        private bool ReserveEnrollment(ZRpc rpc, VaultSession session)
        {
            if (CharacterVaultPlugin.Settings.AllowMultipleCharacters)
            {
                return true;
            }

            if (_enrollments.TryGetValue(session.AccountId, out ZRpc existing) && existing != rpc)
            {
                return false;
            }

            _enrollments[session.AccountId] = rpc;
            return true;
        }

        private void ReleaseEnrollment(ZRpc rpc)
        {
            string account = _enrollments.FirstOrDefault(pair => pair.Value == rpc).Key;
            if (account != null)
            {
                _enrollments.Remove(account);
            }
        }

        private static ZPackage BeginPackage(string transferId, int length, string hash)
        {
            ZPackage package = new ZPackage();
            package.Write(transferId);
            package.Write(length);
            package.Write(hash);
            return package;
        }

        private static ZPackage ChunkPackage(string transferId, byte[] data, int offset)
        {
            int length = Math.Min(ChunkSize, data.Length - offset);
            byte[] chunk = new byte[length];
            Buffer.BlockCopy(data, offset, chunk, 0, length);
            ZPackage package = new ZPackage();
            package.Write(transferId);
            package.Write(offset);
            package.Write(chunk);
            return package;
        }

        private static string NormalizeAccount(string host)
        {
            return host.All(char.IsDigit) ? "Steam_" + host : host;
        }

        private static void Reject(ZRpc rpc, string message)
        {
            CharacterVaultRejection.Reject(rpc, message);
        }

        private static bool IsReady(ZNetPeer peer)
        {
            return peer?.m_rpc != null && peer.IsReady() && peer.m_socket?.IsConnected() == true;
        }

        private static GameObject FindItem(string name)
        {
            return ObjectDB.instance?.m_items.FirstOrDefault(item =>
                string.Equals(item.name, name, StringComparison.OrdinalIgnoreCase));
        }
    }

    internal sealed class VaultSession
    {
        internal VaultSession(string accountId, long characterId, string name, bool newCharacter)
        {
            AccountId = accountId;
            CharacterId = characterId;
            Name = name;
            NewCharacter = newCharacter;
        }

        internal string AccountId { get; }
        internal long CharacterId { get; }
        internal string Name { get; }
        internal bool NewCharacter { get; }
        internal bool Verified { get; set; }
        internal bool Admitted { get; set; }
        internal bool Enrolling { get; set; }
        internal long Revision { get; set; }
    }

    internal sealed class IncomingTransfer
    {
        private readonly byte[] _data;
        private readonly bool[] _blocks;
        private readonly string _hash;
        private readonly string _transferId;

        private IncomingTransfer(string transferId, int length, string hash)
        {
            _transferId = transferId;
            _hash = hash;
            _data = new byte[length];
            _blocks = new bool[(length + 65535) / 65536];
        }

        internal string RequestId { get; set; }

        internal static IncomingTransfer Create(ZPackage package, int maximumLength)
        {
            string id = package.ReadString();
            int length = package.ReadInt();
            string hash = package.ReadString();
            if (string.IsNullOrWhiteSpace(id) || length <= 0 || length > maximumLength || hash.Length != 64)
            {
                throw new InvalidDataException("The profile transfer header is invalid.");
            }

            return new IncomingTransfer(id, length, hash);
        }

        internal void Add(ZPackage package)
        {
            string id = package.ReadString();
            int offset = package.ReadInt();
            byte[] chunk = package.ReadByteArray();
            if (id != _transferId || offset < 0 || offset % 65536 != 0 ||
                chunk.Length == 0 || chunk.Length > 65536 || offset + chunk.Length > _data.Length)
            {
                throw new InvalidDataException("The profile transfer chunk is invalid.");
            }

            int block = offset / 65536;
            if (_blocks[block])
            {
                throw new InvalidDataException("The profile transfer contains a duplicate chunk.");
            }

            Buffer.BlockCopy(chunk, 0, _data, offset, chunk.Length);
            _blocks[block] = true;
        }

        internal byte[] Complete(string transferId)
        {
            if (transferId != _transferId || _blocks.Any(block => !block) ||
                !string.Equals(VaultStorage.Hash(_data), _hash, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The profile transfer is incomplete or corrupted.");
            }

            return _data;
        }
    }
}
