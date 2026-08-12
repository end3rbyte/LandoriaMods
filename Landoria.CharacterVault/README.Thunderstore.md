# CharacterVault

CharacterVault keeps each character on the server. When a player joins, the
server loads its saved copy instead of trusting a local save. This prevents
players from importing or duplicating items with another save or a backup.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## What it does

- Keeps the server copy of each enrolled character as the trusted copy.
- Only accepts a new character created during the current game session.
- Can allow one or several characters per Steam account.
- Can give starting items when a character is enrolled for the first time.
- Saves characters during world saves, disconnects, server stops, and restarts.
- Supports local and Steam Cloud characters on the stable and public test
  versions of Valheim.
- Keeps a previous server copy in case the latest save must be recovered.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Install CharacterVault and ModSentry on the server and on every client. Everyone
must use the same versions.

Server owners can configure the number of characters, starting items, and
graceful shutdowns. See the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.CharacterVault/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
