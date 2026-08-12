# CharacterVault

CharacterVault keeps your Valheim character on the server. When you join, the
server loads its trusted copy, preventing items from being imported or duplicated
with another save or a restored backup.

## Player experience

| Event | What CharacterVault does |
|---|---|
| Join a server | Loads your saved server character before you enter the world. |
| World save | Saves your connected character with the world. |
| Log out | Sends and validates a final character save before disconnecting. |
| Quit from the menu | Saves your character before closing Valheim. |
| Server kick | Saves your character before the server disconnects you. |
| Client crash or lost network | Cannot request a final save because the connection is already lost. |

New characters must be created during the current game session. Depending on
the server settings, an account may use one or several characters and may
receive starting items on first enrollment.

## Compatibility

| Valheim channel | Version | Status |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

CharacterVault supports local and Steam Cloud characters.

## Installation

| Client | Server |
|---|---|
| Required | Required |

Install matching versions of CharacterVault and Landoria.ModSentry on the
server and every client.

Server configuration and graceful restart instructions are available in the
[full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.CharacterVault/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
