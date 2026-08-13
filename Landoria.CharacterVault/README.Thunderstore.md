# CharacterVault

CharacterVault keeps your Valheim character on the server. When you join, the
server loads its trusted copy, preventing items from being imported or duplicated
with another save or a restored backup.

## When characters are saved

| Event | What CharacterVault does |
|---|---|
| First enrollment | Validates and saves a newly created character before allowing it into the world. |
| Automatic world save | Saves every connected character with the world. |
| Manual `save` command | Saves the world and every connected character. |
| Log out | Sends and validates a final character save before disconnecting. |
| Quit from the menu | After entering the world, waits at most 10 seconds to save your character before closing Valheim. |
| Server kick | Saves the affected character before the server disconnects it. |
| Graceful server stop or restart | Saves all connected characters before the final world save and shutdown. |
| Client crash or lost network | Cannot request a final save because the connection is already lost. |

When you join again, CharacterVault loads the latest trusted server copy before
your character enters the world.
The server writes every profile save both as `characters_local/Steam_<id>_<character>.fch` and
as a timestamped `characters_local/backups/` file.
For each character, it retains the 5 most recent backups plus the earliest
backup from each of the next 10 distinct UTC days, up to 15 backups in total.
If a character cannot join, Valheim returns to the main menu and explains why.

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
