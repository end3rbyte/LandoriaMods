# Landoria ModSentry

Guarantees that every player uses the same approved mods, versions, and files, so everyone enters the world under the same conditions.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

- Checks every player's complete mod setup before they enter the world.
- Allows only the exact mods, versions, and files approved by the server.
- Returns the player to the main menu with a clear explanation when a mod must be updated.
- Gives server administrators complete diagnostics for resolving mismatches.
- Supports a fail-closed server-only guest controller for clients without ModSentry.

All other client mods are rejected. If a mod does not match, Valheim returns to the main menu and explains what must be updated instead of remaining on a black loading screen.
There is no setting that enables temporary guests. ModSentry admits one only when
a compatible server-only controller is registered, ready, and accepts the
connection. Without that controller, guest admission remains disabled. Clients
that submit an invalid inventory are always rejected.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

[Read the complete documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.ModSentry/README.md).

## Contact

Report issues through the Landoria website.
