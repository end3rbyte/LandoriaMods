# Landoria ModSentry

ModSentry validates the complete client plugin inventory before the Valheim peer handshake is accepted.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

The server defines two explicit directories under `BepInEx/config`:

- `ModSentry_Required`: every DLL must be installed by the client.
- `ModSentry_Optional`: each DLL may be absent, but must match exactly when installed.

Every other client plugin is rejected. Matching uses the BepInEx GUID, complete plugin version, and exact DLL SHA-256. The client confirms receipt of a specific rejection reason before the server disconnects it, with a short fallback timeout for incompatible clients that cannot acknowledge the message. A rejected client returns to the main menu, where Valheim displays the reason instead of remaining in the loading scene. Player messages identify the affected mod and expected version; client and server logs retain the diagnostic.

## Temporary guests

ModSentry can identify and admit a client that provides no ModSentry inventory
only when a compatible server-only guest controller explicitly registers through
`ModSentryPlugin.RegisterUnverifiedGuestController`. There is no command-line or
BepInEx setting that enables this path.

Admission is fail closed. A missing, incompatible, or unready controller causes
the client to be rejected before entering the world. A controller registration
alone is insufficient: its `IsReady` property must remain true and its admission
callback must complete successfully. Clients that submit an invalid inventory are
always rejected and never enter the guest path.

ModSentry marks admitted connections with a generic temporary-session marker,
lets them bypass the permitted list, and continues to enforce the banned list.
Optional consumers such as CharacterVault can use that marker without knowing
which controller admitted the connection. Physical confinement, world generation,
invulnerability, messaging, and disconnection policy belong to the registered
private server controller rather than this public plugin.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

## Contact

Report issues through the Landoria website.
