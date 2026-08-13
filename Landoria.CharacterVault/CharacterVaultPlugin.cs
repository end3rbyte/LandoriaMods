using System.Collections;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using Landoria.SharedLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

namespace Landoria.CharacterVault
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("Landoria.ModSentry", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class CharacterVaultPlugin : LandoriaPlugin
    {
        private const string PluginGuid = "Landoria.CharacterVault";
        private const string PluginName = "Landoria.CharacterVault";
        private const string PluginVersion = "1.0.11";
        internal static ModLog Log { get; private set; }
        internal static GracefulShutdownCoordinator Coordinator { get; private set; }
        internal static VoluntaryDisconnectCoordinator DisconnectCoordinator { get; private set; }
        internal static ServerDisconnectSaveCoordinator ServerDisconnects { get; private set; }
        internal static CharacterSaveStatusDisplay SaveStatus { get; private set; }
        internal static CharacterVaultPlugin Instance { get; private set; }
        internal static CharacterVaultSettings Settings { get; private set; }
        internal static ProfileTransferService Transfers { get; private set; }
        internal static IWorldCheckpointRequest WorldCheckpoints { get; private set; }
        private ServiceProvider _services;

        private void Awake()
        {
            Instance = this;
            Log = InitializePlugin(PluginGuid);
            Settings = CharacterVaultSettings.Load(Config);
            _services = CharacterVaultServiceRegistration.Build(SynchronizationContext.Current);
            Transfers = _services.GetRequiredService<ProfileTransferService>();
            WorldCheckpoints = _services.GetRequiredService<IWorldCheckpointRequest>();
            Coordinator = _services.GetRequiredService<GracefulShutdownCoordinator>();
            DisconnectCoordinator = _services.GetRequiredService<VoluntaryDisconnectCoordinator>();
            ServerDisconnects = _services.GetRequiredService<ServerDisconnectSaveCoordinator>();
            SaveStatus = _services.GetRequiredService<CharacterSaveStatusDisplay>();
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        internal void Run(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        internal void QuitNextFrame()
        {
            StartCoroutine(QuitAfterCurrentFrame());
        }

        private void Update()
        {
            CharacterVaultRejection.Tick();
        }

        private static IEnumerator QuitAfterCurrentFrame()
        {
            yield return null;
            Application.Quit();
        }

        private void OnDestroy()
        {
            SaveStatus?.Dispose();
            CharacterVaultRejection.Clear();
            _services?.Dispose();
            _services = null;
            DisconnectCoordinator = null;
            ServerDisconnects = null;
            Coordinator = null;
            Transfers = null;
            WorldCheckpoints = null;
            SaveStatus = null;
            Settings = null;
            Instance = null;
            Log?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            ShutdownPlugin();
            Log = null;
        }
    }
}
