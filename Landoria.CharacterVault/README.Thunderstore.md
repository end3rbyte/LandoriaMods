# CharacterVault

CharacterVault stores authoritative Valheim character profiles on the server
and applies the server copy before a player enters the world. This prevents
items from being imported from other servers or duplicated by restoring local
backups.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Applies authoritative server profiles instead of restored or modified local
  copies.
- Accepts first-time enrollment only for a character created during the current
  game session.
- Supports either one or multiple characters per Steam ID.
- Grants server-configured starting items once during initial enrollment.
- Saves connected characters with world saves, disconnects, and graceful
  server shutdowns or restarts.
- Requires ModSentry to enforce the same CharacterVault version on every client
  and the server.
- Uses validated, atomic, revisioned server storage.
- Supports stable and public test Valheim save APIs, including Steam Cloud profiles.
- Migrates old-format profiles forward without downgrading new-format profiles.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Install matching CharacterVault and ModSentry versions on every client and server.

See the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.CharacterVault/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
