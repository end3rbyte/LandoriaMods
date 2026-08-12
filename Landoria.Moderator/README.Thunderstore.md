# Moderator

Provides server-authorized commands and tools for Valheim administrators.

## Features

- Adds an administrator-validated moderator mode that starts disabled.
- Enables god and ghost modes for active moderators.
- Hides and blocks vanilla cheats and `devcommands`.
- Tracks connected players after revealing the map.
- Supports Shift-click map teleportation.
- Displays a replicated green `[Moderator]` name suffix.
- Audits every Moderator command invocation.

## Commands

| Command | Purpose |
|---|---|
| `moderator` | Toggles moderator mode. |
| `exploremap` | Reveals the map and tracks players. |
| `goto <player>` | Teleports to a player. |
| `itemset <biome>` | Applies a vanilla biome item set. |
| `playerlist` | Lists players and administrators. |
| `summon <player>` | Teleports a player to you. |
| `resetmap` | Clears exploration and tracking. |
| `spawn <prefab> [amount] [level] [radius]` | Spawns a Valheim prefab. |

All commands except `moderator` require active moderator mode. The server validates access against `adminlist.txt`.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Matching versions must be installed on the server and every client.

Read the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.Moderator/README.md) on GitHub.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
