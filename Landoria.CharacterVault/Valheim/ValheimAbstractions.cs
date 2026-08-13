using System;
using System.Collections.Generic;
using System.IO;

namespace Landoria.CharacterVault
{
    // ZPackage is concrete and has no mockable domain interface.
    internal interface IValheimPacket
    {
        void Write(long value);
        void Write(string value);
        void Write(bool value);
        void Write(int value);
        void Write(byte[] value);
        long ReadLong();
        string ReadString();
        bool ReadBool();
        int ReadInt();
        byte[] ReadByteArray();
    }

    // ZRpc is concrete. Its socket already uses Valheim's ISocket interface,
    // so this abstraction exposes only the socket values CharacterVault needs.
    internal interface IValheimRpc
    {
        string HostName { get; }
        bool IsConnected { get; }
        void RegisterSignal(string method, Action<IValheimRpc> handler);
        void RegisterString(string method, Action<IValheimRpc, string> handler);
        void RegisterPacket(string method, Action<IValheimRpc, IValheimPacket> handler);
        void SendSignal(string method);
        void SendString(string method, string value);
        void SendPacket(string method, IValheimPacket packet);
    }

    // ZNetPeer is concrete and has no mockable domain interface.
    internal interface IValheimPeer
    {
        IValheimRpc Rpc { get; }
        bool IsConnected { get; }
        string PlayerName { get; }
        bool IsReady { get; }
    }

    internal enum ValheimConnectionStatus
    {
        None,
        Connecting,
        Connected,
        ErrorConnectFailed
    }

    // ZNet is concrete and its global instance is static.
    internal interface IValheimNetwork
    {
        bool IsServer { get; }
        IReadOnlyList<IValheimPeer> Peers { get; }
        IValheimRpc ServerRpc { get; }
        ValheimConnectionStatus ConnectionStatus { get; }
        void Disconnect(IValheimPeer peer);
        void Kick(string hostName);
        void SetExternalError(ValheimConnectionStatus status);
    }

    internal enum ValheimFileSource
    {
        Local,
        Cloud
    }

    // PlayerProfile is concrete and has no mockable domain interface.
    internal interface IValheimPlayerProfile
    {
        long PlayerId { get; }
        string Name { get; }
        string Filename { get; }
        string Path { get; }
        ValheimFileSource FileSource { get; }
        bool Load();
    }

    // Game is concrete and accessed through a global instance.
    internal interface IValheimGame
    {
        IValheimPlayerProfile SelectedProfile { get; }
        void SavePlayerProfile(bool setLogoutPoint);
        void Logout(bool save, bool returnToMainMenu);
    }

    internal interface IValheimItem
    {
        string Name { get; }
    }

    // Inventory is concrete and has no mockable domain interface.
    internal interface IValheimInventory
    {
        bool AddItem(IValheimItem item, int quantity);
    }

    // Player is concrete and exposes the local player through static state.
    internal interface IValheimPlayer
    {
        bool IsLocalPlayer { get; }
        IValheimInventory Inventory { get; }
    }

    // ObjectDB is concrete and accessed through a global instance.
    internal interface IValheimItemDatabase
    {
        IValheimItem Find(string name);
    }

    // FileReader and FileWriter are concrete save helpers.
    internal interface IValheimProfileReader : IDisposable
    {
        Stream Stream { get; }
    }

    internal interface IValheimProfileWriter
    {
        bool CloseSucceeded { get; }
        void Write(byte[] data);
        void Finish();
    }

    // FileHelpers and Utils expose static save operations.
    internal interface IValheimSaveFiles
    {
        IValheimProfileReader OpenReader(IValheimPlayerProfile profile);
        IValheimProfileWriter OpenWriter(string path, ValheimFileSource source);
        void ReplaceOldFile(string currentPath, string nextPath, ValheimFileSource source);
        string GetCharacterPath(ValheimFileSource source, string filename);
        string GetSaveDataPath(ValheimFileSource source);
    }

    // SaveSystem exposes static profile and cache operations.
    internal interface IValheimSaveSystem
    {
        IReadOnlyList<IValheimPlayerProfile> PlayerProfiles { get; }
        void InvalidateCharacterCache();
        IValheimPlayerProfile LoadProfile(string filename, ValheimFileSource source);
    }

    // SyncedList is concrete for Valheim's permission lists.
    internal interface IValheimPermissionList
    {
        int Count { get; }
        bool Contains(string value);
    }

    internal readonly struct ValheimPlatformUserId
    {
        internal ValheimPlatformUserId(string platform, string userId)
        {
            Platform = platform;
            UserId = userId;
        }

        internal string Platform { get; }
        internal string UserId { get; }
    }

    // PlatformUserID is a concrete value type with static parsing helpers.
    internal interface IValheimPlatformIdentity
    {
        bool TryParse(string value, out ValheimPlatformUserId userId);
        ValheimPlatformUserId Create(string platform, string userId);
        string Format(ValheimPlatformUserId userId);
    }

    internal interface IValheimAdapterFactory
    {
        IValheimNetwork Network(object value);
        IValheimPeer Peer(object value);
        IValheimRpc Rpc(object value);
        IValheimPacket Packet(object value);
        IValheimPacket CreatePacket();
        IValheimPlayerProfile Profile(object value);
        IValheimPlayer Player(object value);
        object NativeProfile(IValheimPlayerProfile profile);
    }

    internal interface IValheimEnvironment
    {
        IValheimNetwork Network { get; }
        IValheimGame Game { get; }
        IValheimPlayer LocalPlayer { get; }
        IValheimItemDatabase Items { get; }
        IValheimSaveFiles SaveFiles { get; }
        IValheimSaveSystem SaveSystem { get; }
        bool IsDedicatedServer { get; }
        void Quit();
    }

}
