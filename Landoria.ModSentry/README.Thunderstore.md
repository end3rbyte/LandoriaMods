# Landoria ModSentry

Makes sure every player joins with the correct mod versions, keeping shared worlds consistent and avoiding confusing loading failures.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

- Checks every required and optional mod before entering the world.
- Verifies the exact version and package contents expected by the server.
- Returns the player to the main menu with a clear explanation when a mod must be updated.
- Gives server administrators complete diagnostics for resolving mismatches.

All other client mods are rejected. If a mod does not match, Valheim returns to the main menu and explains what must be updated instead of remaining on a black loading screen.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

[Read the complete documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.ModSentry/README.md).

## Contact

Report issues through the Landoria website.
