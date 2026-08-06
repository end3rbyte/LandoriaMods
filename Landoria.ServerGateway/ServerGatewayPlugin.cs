using System;
using BepInEx;
using BepInEx.Configuration;
using Landoria.SharedLib;

namespace Landoria.ServerGateway
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class ServerGatewayPlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.ServerGateway";
        private const string PluginName = "Landoria.ServerGateway";
        private const string PluginVersion = "1.0.0";
        private const int DefaultPort = 8765;

        private ConfigEntry<int> _port;
        private ConfigEntry<string> _token;
        private LocalGatewayServer _server;
        private float _nextSnapshotTime;
        private int _saveRequested;
        internal static ModLog Log { get; private set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            _port = Config.Bind("RPC", "Port", DefaultPort,
                "Local HTTP port used by the server gateway.");
            _token = Config.Bind("Gateway", "Token", string.Empty,
                "Bearer token required by every gateway endpoint.");
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void Update()
        {
            if (ZNet.instance == null || !ZNet.instance.IsServer())
            {
                return;
            }

            EnsureServerStarted();
            ExecuteQueuedCommands();
            if (UnityEngine.Time.unscaledTime < _nextSnapshotTime)
            {
                return;
            }

            _nextSnapshotTime = UnityEngine.Time.unscaledTime + 1f;
            _server.SetResponse(StatusSnapshot.CreateJson());
        }

        private void EnsureServerStarted()
        {
            if (_server != null)
            {
                return;
            }

            int port = _port.Value;
            if (port < 1 || port > 65535)
            {
                Log.LogWarning($"Invalid RPC port {port}; using {DefaultPort}.");
                port = DefaultPort;
            }

            string token = Environment.GetEnvironmentVariable("LANDORIA_SERVER_GATEWAY_TOKEN") ?? _token.Value;
            _server = new LocalGatewayServer(port, token, QueueSave, Log);
            _server.Start();
        }

        private bool QueueSave()
        {
            return System.Threading.Interlocked.CompareExchange(ref _saveRequested, 1, 0) == 0;
        }

        private void ExecuteQueuedCommands()
        {
            if (System.Threading.Interlocked.Exchange(ref _saveRequested, 0) != 1)
            {
                return;
            }

            if (ZNet.instance == null || !ZNet.instance.IsServer())
            {
                Log.LogWarning("The queued save command was ignored because the server is not ready.");
                return;
            }

            ZNet.instance.SaveWorldAndPlayerProfiles();
            Log.LogInfo("The queued save command was executed.");
        }

        private void OnDestroy()
        {
            _server?.Dispose();
            _server = null;
            ShutdownPlugin();
            Log = null;
        }
    }
}
