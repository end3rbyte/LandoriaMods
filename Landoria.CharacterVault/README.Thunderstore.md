# CharacterVault

CharacterVault saves characters on the server. When a player joins, the
server loads its saved copy instead of trusting a local save. This prevents
players from importing or duplicating items with another save or a backup.

When a player logs out or quits normally, CharacterVault saves the character
and waits for the server to confirm it before disconnecting. Crashes and lost
network connections cannot be delayed for this final save.
Client and server logs share the save request identifier and committed revision
to make each save easy to correlate.
Server kicks are also delayed until the server confirms a final character save.

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
- Logs every successful server save with the character name and revision.
- Supports local and Steam Cloud characters on the stable and public test
  versions of Valheim.
- Keeps a previous server copy in case the latest save must be recovered.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Install CharacterVault and Landoria.ModSentry on the server and on every client.
Everyone must use the same versions.

Server owners can configure the number of characters, starting items, and
graceful shutdowns. See the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.CharacterVault/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
