using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Landoria.ModSentry;

namespace GuestLobbyExample
{
    /// <summary>Runs the server-only guest lobby integration.</summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("Landoria.ModSentry",
        BepInDependency.DependencyFlags.HardDependency)]
    public sealed class GuestLobbyPlugin : BaseUnityPlugin
    {
        private const string PluginGuid = "Example.GuestLobby";
        private const string PluginName = "Guest Lobby Example";
        private const string PluginVersion = "1.0.0";
        private GuestLobbyController _controller;
        private Harmony _harmony;
        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            _controller = new GuestLobbyController();
            ModSentryPlugin.RegisterUnverifiedGuestController(_controller);
        }

        private void Update()
        {
            GuestLobbyController.Tick();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                ModSentryPlugin.UnregisterUnverifiedGuestController(_controller);
            }
            _controller?.ClearGuests();
            _harmony?.UnpatchSelf();
            Log = null;
        }
    }
}
