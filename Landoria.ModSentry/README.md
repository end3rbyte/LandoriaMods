# Landoria ModSentry

ModSentry validates the complete client plugin inventory before the Valheim peer handshake is accepted.

Server-side admission markers are restored after each Valheim player-data sync so
server consumers retain the authoritative guest and verified-ModPack state.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Checks every player's complete plugin list before they enter the world.
- Allows only the exact plugins, versions, and files approved by the server.
- Returns the player to the main menu with a clear explanation when a plugin must be updated.
- Gives server administrators complete diagnostics for resolving mismatches.
- Supports optional lobby routing for clients without ModSentry through a server controller.

The server defines two explicit directories under `BepInEx/config`:

- `ModSentry_Required`: every DLL must be installed by the client.
- `ModSentry_Optional`: each DLL may be absent, but must match exactly when installed.

Every other client plugin is rejected. Matching uses the BepInEx GUID, complete plugin version, and exact DLL SHA-256. The client confirms receipt of a specific rejection reason before the server disconnects it, with a short fallback timeout for incompatible clients that cannot acknowledge the message. A rejected client returns to the main menu, where Valheim displays the reason. Player messages identify the affected mod and expected version; client and server logs retain the diagnostic.

## Temporary guest flow

| Condition or stage | Behavior |
|---|---|
| No ModSentry inventory and a compatible server guest controller is ready | Admit the connection as a temporary guest. |
| Guest destination | Let the server controller choose it; Landoria sends the guest to its protected onboarding lobby. |
| Character data | Mark the temporary session so CharacterVault skips validation, profile loading, and persistence. |
| Invalid inventory or unavailable guest controller | Reject the connection normally. |

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

## Contact

Report issues through the Landoria website.
