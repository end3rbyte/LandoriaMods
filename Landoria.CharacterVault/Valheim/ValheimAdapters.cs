using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Splatform;
using UnityEngine;

namespace Landoria.CharacterVault
{
    internal sealed class ValheimPacketAdapter : IValheimPacket
    {
        internal ValheimPacketAdapter(ZPackage package) => Package = package;
        internal ZPackage Package { get; }

        internal static ValheimPacketAdapter Create() =>
            new ValheimPacketAdapter(new ZPackage());

        public void Write(long value) => Package.Write(value);
        public void Write(string value) => Package.Write(value);
        public void Write(bool value) => Package.Write(value);
        public void Write(int value) => Package.Write(value);
        public void Write(byte[] value) => Package.Write(value);
        public long ReadLong() => Package.ReadLong();
        public string ReadString() => Package.ReadString();
        public bool ReadBool() => Package.ReadBool();
        public int ReadInt() => Package.ReadInt();
        public byte[] ReadByteArray() => Package.ReadByteArray();
    }

    internal sealed class ValheimRpcAdapter : IValheimRpc
    {
        private readonly ZRpc _rpc;

        internal ValheimRpcAdapter(ZRpc rpc) =>
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));

        internal ZRpc Rpc => _rpc;
        public string HostName => _rpc.GetSocket()?.GetHostName();
        public bool IsConnected => _rpc.GetSocket()?.IsConnected() == true;

        public void RegisterSignal(string method, Action<IValheimRpc> handler) =>
            _rpc.Register(method, rpc => handler(new ValheimRpcAdapter(rpc)));

        public void RegisterString(string method, Action<IValheimRpc, string> handler) =>
            _rpc.Register<string>(method,
                (rpc, value) => handler(new ValheimRpcAdapter(rpc), value));

        public void RegisterPacket(string method,
            Action<IValheimRpc, IValheimPacket> handler) =>
            _rpc.Register<ZPackage>(method, (rpc, package) => handler(
                new ValheimRpcAdapter(rpc), new ValheimPacketAdapter(package)));

        public void SendSignal(string method) => _rpc.Invoke(method);
        public void SendString(string method, string value) => _rpc.Invoke(method, value);

        public void SendPacket(string method, IValheimPacket packet) =>
            _rpc.Invoke(method, RequirePacket(packet).Package);

        public override bool Equals(object value) =>
            value is ValheimRpcAdapter other && ReferenceEquals(_rpc, other._rpc);

        public override int GetHashCode() => _rpc.GetHashCode();

        private static ValheimPacketAdapter RequirePacket(IValheimPacket packet) =>
            packet as ValheimPacketAdapter ?? throw new ArgumentException(
                "The packet was not created by the Valheim adapter.", nameof(packet));
    }

    internal sealed class ValheimPeerAdapter : IValheimPeer
    {
        private readonly ZNetPeer _peer;

        internal ValheimPeerAdapter(ZNetPeer peer) =>
            _peer = peer ?? throw new ArgumentNullException(nameof(peer));

        internal ZNetPeer Peer => _peer;
        public IValheimRpc Rpc => _peer.m_rpc == null ? null : new ValheimRpcAdapter(_peer.m_rpc);
        public bool IsConnected => _peer.m_socket?.IsConnected() == true;
        public string PlayerName => _peer.m_playerName;
        public bool IsReady => _peer.IsReady();

        public override bool Equals(object value) =>
            value is ValheimPeerAdapter other && ReferenceEquals(_peer, other._peer);

        public override int GetHashCode() => _peer.GetHashCode();
    }

    internal sealed class ValheimNetworkAdapter : IValheimNetwork
    {
        private readonly ZNet _network;

        internal ValheimNetworkAdapter(ZNet network) =>
            _network = network ?? throw new ArgumentNullException(nameof(network));

        public bool IsServer => _network.IsServer();
        public IReadOnlyList<IValheimPeer> Peers => _network.GetPeers()
            .Select(peer => (IValheimPeer)new ValheimPeerAdapter(peer)).ToArray();
        public IValheimRpc ServerRpc => _network.GetServerRPC() == null
            ? null : new ValheimRpcAdapter(_network.GetServerRPC());
        public ValheimConnectionStatus ConnectionStatus => ParseStatus(
            ZNet.GetConnectionStatus());

        public void Disconnect(IValheimPeer peer) =>
            _network.Disconnect(RequirePeer(peer).Peer);

        public void Kick(string hostName) => _network.Kick(hostName);

        public void SetExternalError(ValheimConnectionStatus status) =>
            ZNet.SetExternalError((ZNet.ConnectionStatus)Enum.Parse(
                typeof(ZNet.ConnectionStatus), status.ToString()));

        private static ValheimPeerAdapter RequirePeer(IValheimPeer peer) =>
            peer as ValheimPeerAdapter ?? throw new ArgumentException(
                "The peer was not created by the Valheim adapter.", nameof(peer));

        private static ValheimConnectionStatus ParseStatus(ZNet.ConnectionStatus status)
        {
            return Enum.TryParse(status.ToString(), out ValheimConnectionStatus value)
                ? value : ValheimConnectionStatus.None;
        }
    }

    internal sealed class ValheimPlayerProfileAdapter : IValheimPlayerProfile
    {
        private readonly PlayerProfile _profile;

        internal ValheimPlayerProfileAdapter(PlayerProfile profile) =>
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));

        internal PlayerProfile Profile => _profile;
        public long PlayerId => _profile.GetPlayerID();
        public string Name => _profile.GetName();
        public string Filename => _profile.GetFilename();
        public string Path => _profile.GetPath();
        public ValheimFileSource FileSource => ValheimSource.FromNative(_profile.m_fileSource);
        public bool Load() => _profile.Load();
    }

    internal sealed class ValheimGameAdapter : IValheimGame
    {
        private readonly Game _game;

        internal ValheimGameAdapter(Game game) =>
            _game = game ?? throw new ArgumentNullException(nameof(game));

        public IValheimPlayerProfile SelectedProfile => _game.GetPlayerProfile() == null
            ? null : new ValheimPlayerProfileAdapter(_game.GetPlayerProfile());
        public void SavePlayerProfile(bool setLogoutPoint) =>
            _game.SavePlayerProfile(setLogoutPoint);
        public void Logout(bool save, bool returnToMainMenu) =>
            _game.Logout(save, returnToMainMenu);
    }

    internal sealed class ValheimItemAdapter : IValheimItem
    {
        internal ValheimItemAdapter(GameObject item) =>
            Item = item ?? throw new ArgumentNullException(nameof(item));

        internal GameObject Item { get; }
        public string Name => Item.name;
    }

    internal sealed class ValheimInventoryAdapter : IValheimInventory
    {
        private readonly Inventory _inventory;

        internal ValheimInventoryAdapter(Inventory inventory) =>
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));

        public bool AddItem(IValheimItem item, int quantity) =>
            _inventory.AddItem(RequireItem(item).Item, quantity);

        private static ValheimItemAdapter RequireItem(IValheimItem item) =>
            item as ValheimItemAdapter ?? throw new ArgumentException(
                "The item was not created by the Valheim adapter.", nameof(item));
    }

    internal sealed class ValheimPlayerAdapter : IValheimPlayer
    {
        private readonly Player _player;

        internal ValheimPlayerAdapter(Player player) =>
            _player = player ?? throw new ArgumentNullException(nameof(player));

        public bool IsLocalPlayer => ReferenceEquals(_player, Player.m_localPlayer);
        public IValheimInventory Inventory => new ValheimInventoryAdapter(_player.GetInventory());
    }

    internal sealed class ValheimItemDatabaseAdapter : IValheimItemDatabase
    {
        private readonly ObjectDB _database;

        internal ValheimItemDatabaseAdapter(ObjectDB database) =>
            _database = database ?? throw new ArgumentNullException(nameof(database));

        public IValheimItem Find(string name)
        {
            GameObject item = _database.m_items.FirstOrDefault(candidate =>
                string.Equals(candidate.name, name, StringComparison.OrdinalIgnoreCase));
            return item == null ? null : new ValheimItemAdapter(item);
        }
    }

    internal sealed class ValheimProfileReaderAdapter : IValheimProfileReader
    {
        private readonly FileReader _reader;

        internal ValheimProfileReaderAdapter(FileReader reader) =>
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));

        public Stream Stream => _reader.m_binary.BaseStream;
        public void Dispose() => _reader.Dispose();
    }

    internal sealed class ValheimProfileWriterAdapter : IValheimProfileWriter
    {
        private readonly FileWriter _writer;

        internal ValheimProfileWriterAdapter(FileWriter writer) =>
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        public bool CloseSucceeded =>
            _writer.Status == FileWriter.WriterStatus.CloseSucceeded;
        public void Write(byte[] data) => _writer.m_binary.Write(data);
        public void Finish() => _writer.Finish();
    }

    internal sealed class ValheimSaveFilesAdapter : IValheimSaveFiles
    {
        public IValheimProfileReader OpenReader(IValheimPlayerProfile profile)
        {
            ValheimPlayerProfileAdapter native = RequireProfile(profile);
            return new ValheimProfileReaderAdapter(
                new FileReader(native.Profile.GetPath(), native.Profile.m_fileSource));
        }

        public IValheimProfileWriter OpenWriter(string path, ValheimFileSource source) =>
            new ValheimProfileWriterAdapter(
                SaveApiCompatibility.CreateWriter(path, ValheimSource.ToNative(source)));

        public void ReplaceOldFile(string currentPath, string nextPath,
            ValheimFileSource source) => SaveApiCompatibility.ReplaceOldFile(
                currentPath, nextPath, ValheimSource.ToNative(source));

        public string GetCharacterPath(ValheimFileSource source, string filename) =>
            SaveApiCompatibility.GetCharacterPath(ValheimSource.ToNative(source), filename);

        public string GetSaveDataPath(ValheimFileSource source) =>
            Utils.GetSaveDataPath(ValheimSource.ToNative(source));

        private static ValheimPlayerProfileAdapter RequireProfile(IValheimPlayerProfile profile) =>
            profile as ValheimPlayerProfileAdapter ?? throw new ArgumentException(
                "The profile was not created by the Valheim adapter.", nameof(profile));
    }

    internal sealed class ValheimSaveSystemAdapter : IValheimSaveSystem
    {
        public IReadOnlyList<IValheimPlayerProfile> PlayerProfiles => SaveSystem
            .GetAllPlayerProfiles()
            .Select(profile => (IValheimPlayerProfile)new ValheimPlayerProfileAdapter(profile))
            .ToArray();

        public void InvalidateCharacterCache() =>
            SaveApiCompatibility.InvalidateCharacterCache();

        public IValheimPlayerProfile LoadProfile(string filename, ValheimFileSource source)
        {
            PlayerProfile profile = new PlayerProfile(filename, ValheimSource.ToNative(source));
            return new ValheimPlayerProfileAdapter(profile);
        }
    }

    internal sealed class ValheimPermissionListAdapter : IValheimPermissionList
    {
        private readonly SyncedList _list;

        internal ValheimPermissionListAdapter(SyncedList list) =>
            _list = list ?? throw new ArgumentNullException(nameof(list));

        public int Count => _list.Count();
        public bool Contains(string value) => _list.Contains(value);
    }

    internal sealed class ValheimPlatformIdentityAdapter : IValheimPlatformIdentity
    {
        public bool TryParse(string value, out ValheimPlatformUserId userId)
        {
            bool parsed = PlatformUserID.TryParse(value, out PlatformUserID native);
            userId = parsed ? FromNative(native) : default;
            return parsed;
        }

        public ValheimPlatformUserId Create(string platform, string userId)
        {
            Platform nativePlatform = (Platform)Enum.Parse(typeof(Platform), platform);
            return FromNative(new PlatformUserID(nativePlatform, userId));
        }

        public string Format(ValheimPlatformUserId userId) =>
            ToNative(userId).ToString();

        private static ValheimPlatformUserId FromNative(PlatformUserID userId) =>
            new ValheimPlatformUserId(userId.m_platform.ToString(), userId.m_userID.ToString());

        private static PlatformUserID ToNative(ValheimPlatformUserId userId) =>
            new PlatformUserID((Platform)Enum.Parse(typeof(Platform), userId.Platform),
                userId.UserId);
    }

    internal static class ValheimSource
    {
        internal static ValheimFileSource FromNative(FileHelpers.FileSource source) =>
            Enum.TryParse(source.ToString(), true, out ValheimFileSource value)
                ? value : ValheimFileSource.Local;

        internal static FileHelpers.FileSource ToNative(ValheimFileSource source) =>
            (FileHelpers.FileSource)Enum.Parse(
                typeof(FileHelpers.FileSource), source.ToString(), true);
    }

    internal sealed class ValheimAdapterFactory : IValheimAdapterFactory
    {
        public IValheimNetwork Network(object value) =>
            new ValheimNetworkAdapter((ZNet)value);
        public IValheimPeer Peer(object value) => new ValheimPeerAdapter((ZNetPeer)value);
        public IValheimRpc Rpc(object value) => new ValheimRpcAdapter((ZRpc)value);
        public IValheimPacket Packet(object value) =>
            new ValheimPacketAdapter((ZPackage)value);
        public IValheimPacket CreatePacket() => ValheimPacketAdapter.Create();
        public IValheimPlayerProfile Profile(object value) =>
            new ValheimPlayerProfileAdapter((PlayerProfile)value);
        public IValheimPlayer Player(object value) => new ValheimPlayerAdapter((Player)value);

        public object NativeProfile(IValheimPlayerProfile profile) =>
            RequireProfile(profile).Profile;

        private static ValheimPlayerProfileAdapter RequireProfile(
            IValheimPlayerProfile profile) => profile as ValheimPlayerProfileAdapter ??
                throw new ArgumentException(
                    "The profile was not created by the Valheim adapter.", nameof(profile));
    }

    internal sealed class ValheimEnvironment : IValheimEnvironment
    {
        public IValheimNetwork Network => ZNet.instance == null
            ? null : new ValheimNetworkAdapter(ZNet.instance);
        public IValheimGame Game => Game.instance == null
            ? null : new ValheimGameAdapter(Game.instance);
        public IValheimPlayer LocalPlayer => Player.m_localPlayer == null
            ? null : new ValheimPlayerAdapter(Player.m_localPlayer);
        public IValheimItemDatabase Items => ObjectDB.instance == null
            ? null : new ValheimItemDatabaseAdapter(ObjectDB.instance);
        public IValheimSaveFiles SaveFiles { get; } = new ValheimSaveFilesAdapter();
        public IValheimSaveSystem SaveSystem { get; } = new ValheimSaveSystemAdapter();
        public bool IsDedicatedServer => Application.isBatchMode;
        public void Quit() => Application.Quit();
    }
}
