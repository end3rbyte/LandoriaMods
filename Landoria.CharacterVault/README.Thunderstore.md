# CharacterVault

CharacterVault stores authoritative Valheim character profiles on the server and applies the server copy before a player enters the world.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Requires ModSentry verification before character enrollment.
- Ignores restored or modified local backups for registered characters.
- Supports one or multiple characters per Steam ID.
- Grants server-configured starting items exactly once.
- Saves characters with world saves and graceful server shutdowns.
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
