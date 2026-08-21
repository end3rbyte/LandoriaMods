# Guest lobby example

This directory contains the files for a complete minimal guest lobby implementation.

The lobby is placed 40 metres from the Start Temple and 70 metres above the
highest terrain measured within 40 metres of its target position.

| File | Purpose |
|---|---|
| `GuestLobbyPlugin.cs` | Starts the server-only integration and handles cleanup. |
| `GuestLobbyController.cs` | Handles ModSentry admission, guest sessions, and continuous confinement. |
| `GuestLobbyGenerator.cs` | Locates, builds, and recreates the lobby. |
| `GuestLobbyProtection.cs` | Protects the structure, brazier, rug, sign, ownership, and build boundary. |
| `GuestLobbyPatches.cs` | Contains the separate Harmony patch classes in one file. |
| `GuestLobbyUtility.cs` | Provides the shared version-compatible hash helper. |
| `GuestLobbyExample.csproj` | Builds the example as a separate server-only plugin. |

## Key code

The plugin entry point registers one guest controller with ModSentry:

```csharp
private void Awake()
{
    Log = Logger;
    _harmony = new Harmony(PluginGuid);
    _harmony.PatchAll();
    _controller = new GuestLobbyController();
    ModSentryPlugin.RegisterUnverifiedGuestController(_controller);
}
```

The controller implements all five members of the ModSentry guest-controller
contract:

```csharp
public int ProtocolVersion => ModSentryPlugin.GuestControllerProtocolVersion;

public bool IsReady => GuestLobbyGenerator.IsOperational &&
    GuestLobbyGenerator.TryGetPosition(out _);

public void OnGuestAdmitted(ZRpc rpc)
{
    if (!IsReady)
    {
        throw new System.InvalidOperationException(
            "The guest lobby is unavailable.");
    }
    Guests[rpc] = new GuestState();
    GuestLobbyPlugin.Log.LogInfo("Started tracking an admitted guest.");
}

public void OnGuestDisconnected(ZRpc rpc)
{
    if (Guests.Remove(rpc))
    {
        GuestLobbyPlugin.Log.LogInfo(
            "Stopped tracking a disconnected guest.");
    }
}

public void ClearGuests()
{
    if (Guests.Count == 0)
    {
        return;
    }
    Guests.Clear();
    GuestLobbyPlugin.Log.LogInfo("Cleared all tracked guest sessions.");
}
```

`ProtocolVersion` confirms API compatibility. `IsReady` prevents admission
until the lobby exists. The other three members add, remove, or clear tracked
guest sessions as connections and the plugin lifecycle change.

The server sends a guest back to the lobby when confinement detects that the
character is outside its protected boundary:

```csharp
private static void SendTeleport(ZNetPeer peer, GuestState state, Vector3 lobby)
{
    float now = Time.unscaledTime;
    if (now < state.NextTeleportAt || ZRoutedRpc.instance == null)
    {
        return;
    }
    state.NextTeleportAt = now + RetrySeconds;
    ZRoutedRpc.instance.InvokeRoutedRPC(0L, peer.m_characterID,
        "RPC_TeleportTo", lobby, Quaternion.identity, true);
}
```
