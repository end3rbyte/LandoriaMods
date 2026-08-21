# Landoria ModSentry

ModSentry checks a player's mod files before allowing them into the world.

## Features

- Checks every DLL under `BepInEx/plugins`, including plugin libraries.
- Requires every file approved by the server to have the expected version and SHA-256 hash.
- Rejects unapproved DLLs.
- Returns the player to the main menu with a clear explanation when a file must be corrected.
- Supports an optional temporary-guest lobby for clients without ModSentry.

If a plugin requires another DLL, the server can require and verify both files.

## Compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Installation

| Client required | Dedicated server required | Player-hosted server |
|---|---|---|
| Yes | Yes | Not supported |

[Read the complete documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.ModSentry/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
