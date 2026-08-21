# ModSentry guest lobby integration

ModSentry can admit a player who does not have ModSentry as a guest. This allows
a server-side mod to send the player to a restricted lobby located in the same
world. For example, the lobby can explain how to install the required mods or
ModPack. The guest lobby must be compatible with a vanilla Valheim client.

ModSentry does not create the lobby or move the player. The server-side guest
lobby mod is responsible for its location, construction, teleportation,
protection, and guest sessions.

## Controller contract

The server-only mod must implement `IUnverifiedGuestController`:

| Member | Purpose |
|---|---|
| `ProtocolVersion` | Return `ModSentryPlugin.GuestControllerProtocolVersion`. |
| `IsReady` | Return `true` only after the lobby has been created in the active world. |
| `OnGuestAdmitted(ZRpc rpc)` | Register the connection and begin its guest session. |
| `OnGuestDisconnected(ZRpc rpc)` | Remove the connection and its state. |
| `ClearGuests()` | Remove all guest state when the mod stops. |

## Complete minimal example

A complete minimal implementation is available in the [`Example`](https://github.com/landoria-gaming/LandoriaMods/tree/main/Landoria.ModSentry/Example) subdirectory.
