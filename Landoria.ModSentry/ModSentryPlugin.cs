using BepInEx;
using BepInEx.Configuration;
using Landoria.SharedLib;

namespace Landoria.ModSentry
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class ModSentryPlugin : LandoriaPlugin
    {
        internal const string InventoryRpc = "Landoria_ModSentry_Inventory";
        internal const string RejectionRpc = "Landoria_ModSentry_Rejection";
        internal const string RejectionAckRpc = "Landoria_ModSentry_RejectionAck";
        internal const int ProtocolVersion = 1;
        private const string PluginGuid = "Landoria.ModSentry";
        private const string PluginName = "Landoria.ModSentry";
        private const string PluginVersion = "1.0.9";

        internal static ModLog Log { get; private set; }
        internal static PluginPolicy Policy { get; private set; }
        internal static ConfigEntry<bool> AllowUnverifiedGuests { get; private set; }
        internal static ConfigEntry<string> GuestMessage { get; private set; }

        private void Awake()
        {
            Log = InitializePlugin(PluginGuid);
            BindSettings();
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        private void BindSettings()
        {
            AllowUnverifiedGuests = Config.Bind("Guest admission", "Allow unverified guests",
                false, "Temporarily admit clients that do not provide a ModSentry inventory.");
            GuestMessage = Config.Bind("Guest admission", "Message",
                GuestAdmissionMessages.DefaultRegistration,
                "Message shown to an unverified guest before disconnection.");
        }

        internal static PluginPolicy EnsurePolicy()
        {
            if (Policy == null)
            {
                Policy = PluginPolicyLoader.Load();
                Log.LogInfo($"Loaded {Policy.Required.Count} required and " +
                            $"{Policy.Optional.Count} optional client mod policies.");
            }

            return Policy;
        }

        private void Update()
        {
            PendingDisconnects.Tick();
            GuestAdmissions.Tick();
            ClientMessage.Tick();
        }

        private void OnDestroy()
        {
            Log?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            HandshakeState.Clear();
            PendingDisconnects.Clear();
            GuestAdmissions.Clear();
            ClientMessage.Clear();
            Policy = null;
            AllowUnverifiedGuests = null;
            GuestMessage = null;
            ShutdownPlugin();
            Log = null;
        }
    }
}
