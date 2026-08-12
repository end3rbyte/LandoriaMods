# Moderator

Provides server-authorized commands and tools for Valheim administrators.

## Features

- Adds an administrator-validated moderator mode that starts disabled.
- Enables god and ghost modes while moderator mode is active.
- Hides and blocks vanilla cheat commands and `devcommands`.
- Tracks connected players on the map after `exploremap`.
- Supports Shift-click map teleportation for active moderators.
- Displays a replicated green `[Moderator]` name suffix.
- Records every Moderator command invocation at Info level.

## Commands

| Command | Purpose | Access |
|---|---|---|
| `moderator` | Enables or disables moderator mode for the session. | Administrator |
| `exploremap` | Reveals the map and continuously displays connected players. | Active moderator |
| `goto <player>` | Teleports to another player. | Active moderator |
| `itemset <biome>` | Replaces current items with the selected vanilla biome item set. | Active moderator |
| `playerlist` | Lists online players and identifies administrators. | Active moderator |
| `summon <player>` | Teleports another player to the moderator. | Active moderator |
| `resetmap` | Clears local map exploration and player tracking. | Active moderator |
| `spawn <prefab> [amount] [level] [radius]` | Spawns a Valheim prefab. | Active moderator |

`itemset` supports `meadows`, `blackforest`, `swamp`, `mountain`, and `plains`. Existing items are placed in a tombstone.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Matching versions must be installed on the server and every client.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
