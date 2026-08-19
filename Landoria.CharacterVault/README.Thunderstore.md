# CharacterVault

CharacterVault keeps your Valheim character on the server. When you join, the
server loads its trusted copy, preventing items from being imported or duplicated
with another save or a restored backup.

## When characters are saved

| Event | What CharacterVault does |
|---|---|
| First enrollment | Validates and saves the new character after they safely enter the world. |
| Temporary guest admitted by ModSentry | Does not validate, load, or save the character while the server sends the guest to its configured destination. |
| Automatic world save | Saves every connected character with the world. |
| Manual `save` command | Saves the world and every connected character. |
| Pause-menu Save button | Saves the trusted server copy while keeping the familiar Valheim save behavior. |
| Log out | Sends and validates a final character save before disconnecting. |
| Quit from the menu | After entering the world, waits at most 10 seconds to save your character before closing Valheim. |
| Server kick | Saves the affected character before the server disconnects it. |
| Graceful server stop or restart (optional) | Saves all connected characters before the final world save and shutdown. Requires Linux server configuration. |
| Client crash or lost network | Cannot request a final save because the connection is already lost. |

## Features

- The server loads its latest trusted copy before your character enters the world; local saves still work normally.
- A status below the minimap shows when the character is saving and confirms when the save is complete.
- `Failed` appears when the server does not confirm the save in time.
- The server keeps up to 15 automatic backups per character, including the 5 most recent saves and older daily snapshots.
- Rejected accounts are never saved and receive a clear error. New characters must be created during the current game session; character limits and starting items depend on the server.

## Compatibility

| Valheim channel | Version | Status |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

CharacterVault supports players using Valheim on Steam and Xbox.
CharacterVault supports local and Steam Cloud characters.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

Install matching versions of CharacterVault and Landoria.ModSentry on the
server and every client.

Graceful server stop and restart support is optional and requires configuration
on the Linux server. Setup instructions are available in the
[full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.CharacterVault/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
