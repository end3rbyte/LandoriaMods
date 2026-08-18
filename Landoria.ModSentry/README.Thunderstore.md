# Landoria ModSentry

Guarantees that every player uses the same approved mods, versions, and files, so everyone enters the world under the same conditions.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Checks every player's complete mod setup before they enter the world.
- Allows only the exact mods, versions, and files approved by the server.
- Returns the player to the main menu with a clear explanation when a mod must be updated.
- Gives server administrators complete diagnostics for resolving mismatches.
- Supports optional lobby routing for clients without ModSentry through a server controller.

All other client mods are rejected. If a mod does not match, Valheim returns to the main menu and explains what must be updated.

## Temporary guests

| Stage | What happens |
|---|---|
| A client sends no ModSentry inventory | A compatible server controller may admit the connection as a temporary guest. |
| The guest enters | Landoria sends the guest to its protected onboarding lobby. |
| CharacterVault handles the session | It skips character validation, profile loading, and every save. |
| The inventory is invalid or the controller is unavailable | ModSentry rejects the connection normally. |

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

[Read the complete documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.ModSentry/README.md).

## Contact

Report issues through the Landoria website.
