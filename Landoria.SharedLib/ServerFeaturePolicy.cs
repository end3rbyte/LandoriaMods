using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;

namespace Landoria.SharedLib
{
    public sealed class ServerFeaturePolicy : IDisposable
    {
        private static readonly List<ServerFeaturePolicy> Policies =
            new List<ServerFeaturePolicy>();

        private readonly ConfigEntry<bool> _configuredEnabled;
        private readonly string _requestRpc;
        private readonly string _responseRpc;
        private readonly string _pluginGuid;
        private readonly string _pluginVersion;
        private readonly ModLog _log;
        private ZRoutedRpc _registeredRpc;
        private bool _receivedServerState;
        private bool _serverEnabled;

        public ServerFeaturePolicy(
            ConfigFile config,
            string pluginGuid,
            string pluginVersion,
            ModLog log,
            bool defaultEnabled)
        {
            _configuredEnabled = config.Bind(
                "General",
                "Enabled",
                defaultEnabled,
                "Controls whether this feature is enabled by the server. " +
                "Defaults to enabled on dedicated servers and disabled otherwise.");
            _requestRpc = pluginGuid + ".FeaturePolicy.Request";
            _responseRpc = pluginGuid + ".FeaturePolicy.Response";
            _pluginGuid = pluginGuid;
            _pluginVersion = pluginVersion;
            _log = log;
            Policies.Add(this);
        }

        public bool IsEnabled
        {
            get
            {
                if (ZNet.instance != null && ZNet.instance.IsServer())
                {
                    return _configuredEnabled.Value;
                }

                return _receivedServerState && _serverEnabled;
            }
        }

        public void Dispose()
        {
            ResetSession();
            Policies.Remove(this);
        }

        internal static void RegisterAndSynchronizeAll()
        {
            foreach (ServerFeaturePolicy policy in Policies)
            {
                policy.RegisterRpcs();
                policy.RequestServerState();
            }
        }

        internal static void ResetAll()
        {
            foreach (ServerFeaturePolicy policy in Policies)
            {
                policy.ResetSession();
            }
        }

        private void RegisterRpcs()
        {
            ZRoutedRpc rpc = ZRoutedRpc.instance;
            if (rpc == null || ReferenceEquals(rpc, _registeredRpc))
            {
                return;
            }

            rpc.Register<string>(_requestRpc, ReceiveRequest);
            rpc.Register<bool, string>(_responseRpc, ReceiveResponse);
            _registeredRpc = rpc;
            _log.LogDebug("Server feature policy RPCs registered.");
        }

        private void RequestServerState()
        {
            if (ZNet.instance == null || ZNet.instance.IsServer() ||
                ZRoutedRpc.instance == null)
            {
                return;
            }

            _receivedServerState = false;
            _serverEnabled = false;
            ZRoutedRpc.instance.InvokeRoutedRPC(_requestRpc, _pluginVersion);
            _log.LogDebug("Requested the server feature policy.");
        }

        private void ReceiveRequest(long sender, string clientVersion)
        {
            ZNetPeer peer = ZNet.instance?.GetPeer(sender);
            if (ZNet.instance == null || !ZNet.instance.IsServer() ||
                ZRoutedRpc.instance == null || peer == null)
            {
                return;
            }

            bool enabled = _configuredEnabled.Value;
            ZRoutedRpc.instance.InvokeRoutedRPC(
                sender,
                _responseRpc,
                enabled,
                _pluginVersion);
            if (enabled && !VersionsMatch(clientVersion))
            {
                LogVersionMismatch("server", clientVersion, _pluginVersion);
                ZNet.instance.Disconnect(peer);
                return;
            }

            _log.LogDebug($"Sent server feature policy to peer {sender}: {enabled}.");
        }

        private void ReceiveResponse(long sender, bool enabled, string serverVersion)
        {
            ZNetPeer senderPeer = ZNet.instance?.GetPeer(sender);
            if (ZNet.instance == null || ZNet.instance.IsServer() ||
                senderPeer == null ||
                !ReferenceEquals(senderPeer, ZNet.instance.GetServerPeer()))
            {
                return;
            }

            if (enabled && !VersionsMatch(serverVersion))
            {
                LogVersionMismatch("client", _pluginVersion, serverVersion);
                ZNet.instance.Disconnect(senderPeer);
                return;
            }

            _receivedServerState = true;
            _serverEnabled = enabled;
            _log.LogDebug($"Received server feature policy: {enabled}.");
        }

        private bool VersionsMatch(string remoteVersion) =>
            string.Equals(_pluginVersion, remoteVersion, StringComparison.Ordinal);

        private void LogVersionMismatch(
            string side,
            string clientVersion,
            string serverVersion)
        {
            _log.LogError(
                $"Disconnecting incompatible peer for {_pluginGuid} on {side}: " +
                $"client version '{clientVersion}', server version '{serverVersion}'.");
        }

        private void ResetSession()
        {
            _registeredRpc = null;
            _receivedServerState = false;
            _serverEnabled = false;
        }
    }

    internal static class FeaturePolicyHarmony
    {
        internal static Harmony Apply(string pluginGuid)
        {
            Harmony harmony = new Harmony(pluginGuid + ".FeaturePolicy");
            harmony.CreateClassProcessor(typeof(FeaturePolicyGameStartPatch)).Patch();
            harmony.CreateClassProcessor(typeof(FeaturePolicyLocalPlayerPatch)).Patch();
            harmony.CreateClassProcessor(typeof(FeaturePolicyDisconnectPatch)).Patch();
            return harmony;
        }
    }

    [HarmonyPatch(typeof(Game), "Start")]
    internal static class FeaturePolicyGameStartPatch
    {
        private static void Postfix() =>
            ServerFeaturePolicy.RegisterAndSynchronizeAll();
    }

    [HarmonyPatch(typeof(Player), "SetLocalPlayer")]
    internal static class FeaturePolicyLocalPlayerPatch
    {
        private static void Postfix() =>
            ServerFeaturePolicy.RegisterAndSynchronizeAll();
    }

    [HarmonyPatch(typeof(ZNet), "OnDestroy")]
    internal static class FeaturePolicyDisconnectPatch
    {
        private static void Postfix() => ServerFeaturePolicy.ResetAll();
    }
}
