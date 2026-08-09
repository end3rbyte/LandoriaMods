using System;
using BepInEx;
using BepInEx.Configuration;
using Landoria.SharedLib;
using UnityEngine;

namespace Landoria.AfkDetector
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class AfkDetectorPlugin : LandoriaPlugin
    {
        internal const string DisconnectReasonRpc = "Landoria_AfkDisconnectReason";
        private const string PluginGuid = "Landoria.AfkDetector";
        private const string PluginName = "Landoria.AfkDetector";
        private const string PluginVersion = "1.0.0";
        private const int DefaultTimeoutMinutes = 30;
        private const string TimeoutArgument = "--afktimeout";
        private const float DefaultMovementTolerance = 0.75f;
        private const float ScanIntervalSeconds = 2f;

        private ConfigEntry<int> _timeoutMinutes;
        private int? _commandLineTimeoutMinutes;
        private ConfigEntry<float> _movementTolerance;
        private ActivityMonitor _monitor;
        private float _nextScan;
        internal static AfkDetectorPlugin Instance { get; private set; }
        internal static ModLog Log { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = InitializePlugin(PluginGuid);
            BindConfiguration();
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void BindConfiguration()
        {
            _timeoutMinutes = Config.Bind("Detection", "TimeoutMinutes", DefaultTimeoutMinutes,
                "Minutes without movement or chat before the server disconnects a player.");
            _commandLineTimeoutMinutes = ReadCommandLineTimeout();
            _movementTolerance = Config.Bind("Detection", "MovementToleranceMeters",
                DefaultMovementTolerance,
                "Minimum distance that resets the inactivity timer and filters position jitter.");
        }

        private void Update()
        {
            if (!IsReadyServer() || Time.unscaledTime < _nextScan)
            {
                return;
            }

            _nextScan = Time.unscaledTime + ScanIntervalSeconds;
            EnsureMonitor().Update(ZNet.instance.GetPeers(), Time.unscaledTime);
        }

        private ActivityMonitor EnsureMonitor()
        {
            float timeout = Mathf.Max(1, EffectiveTimeoutMinutes()) * 60f;
            float tolerance = Mathf.Max(0.1f, _movementTolerance.Value);
            if (_monitor == null)
            {
                _monitor = new ActivityMonitor(timeout, tolerance, DisconnectPlayer);
            }
            else
            {
                _monitor.Configure(timeout, tolerance);
            }
            return _monitor;
        }

        private int EffectiveTimeoutMinutes()
        {
            return _commandLineTimeoutMinutes ?? _timeoutMinutes.Value;
        }

        private static int? ReadCommandLineTimeout()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(arguments[index], TimeoutArgument,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return ParseCommandLineTimeout(arguments, index);
            }

            return null;
        }

        private static int? ParseCommandLineTimeout(string[] arguments, int index)
        {
            if (index + 1 < arguments.Length &&
                int.TryParse(arguments[index + 1], out int minutes) && minutes >= 1)
            {
                Log.LogInfo($"Received command-line switch: {TimeoutArgument} {minutes}.");
                return minutes;
            }

            Log.LogWarning($"Invalid {TimeoutArgument} value; using the BepInEx configuration.");
            return null;
        }

        internal void RecordChat(long peerId)
        {
            if (IsReadyServer())
            {
                EnsureMonitor().RecordChat(peerId, Time.unscaledTime);
            }
        }

        private static bool IsReadyServer()
        {
            return ZNet.instance != null && ZNet.instance.IsServer();
        }

        private static void DisconnectPlayer(ZNetPeer peer)
        {
            string message = "You were disconnected because you were inactive for too long.";
            peer.m_rpc.Invoke(DisconnectReasonRpc, message);
            ZNet.instance.Kick(peer.m_socket.GetHostName());
            Log.LogInfo($"Disconnected inactive player {peer.m_playerName}.");
        }

        private void OnDestroy()
        {
            Log?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            ShutdownPlugin();
            _monitor = null;
            Instance = null;
            Log = null;
        }
    }
}
