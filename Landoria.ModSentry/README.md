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

Dedicated servers may temporarily admit clients that provide no ModSentry inventory.
This option is disabled by default. When enabled, the player receives the configured
registration message in chat and a center-screen countdown, then is disconnected
two minutes after their character enters the world. The configured prison position
is imposed on the player's first spawn and every respawn during that session.
Clients that submit an invalid
inventory remain rejected before admission.
Temporary guests bypass the server permitted list during this grace period, while
the banned list remains fully enforced.

| BepInEx setting | Default | Purpose |
|---|---|---|
| `Guest admission.Allow unverified guests` | `false` | Enables the temporary guest path. |
| `Guest admission.Message` | Generic required-modpack notice | Sets the chat and center-screen message. |
| `Guest admission.Prison position` | Empty | Optional invariant `X,Y,Z` spawn and respawn position; empty uses the world's Stone Temple. |

Server logs record admission, countdown start, message delivery, and disconnection.
A vanilla guest cannot produce ModSentry client logs because the plugin is not installed.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

## Contact

Report issues through the Landoria website.
