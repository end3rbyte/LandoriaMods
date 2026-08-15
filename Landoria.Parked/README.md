# Parked

Protects player-built pieces according to their creator's Socialize group activity.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Uses vanilla rain wear and fuel consumption while a piece's creator or one of their group members is connected.
- Stops rain wear and fuel consumption for player-built pieces while the creator and their entire group are offline.
- Prevents creatures from targeting or damaging those pieces while the creator and their entire group are offline.
- Restricts interaction with player-built pieces to their creator and members of the creator's group.
- Shows the creator's name while hovering over a player-built piece that the local player cannot use.
- Supports dedicated servers.

Pieces without an identifiable player creator retain vanilla behavior.
Fuel does not catch up retroactively when a protected fireplace or torch is loaded again.

Piece interaction protection includes normal use, using an item on a piece, containers, doors, crafting stations, adding resources or fuel, repairs, hammer removal, and damage caused by a player. Creatures use vanilla targeting and damage whenever the creator or a group member is online, and ignore the piece while the entire group is offline. Environmental damage and interactions with natural or creatorless objects retain vanilla behavior.

Denied interactions are silent. Hovering an inaccessible piece shows its creator instead. New pieces store the creator name directly; legacy pieces use names already known by Parked and may display `Unknown creator` until the creator is identified.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

Install matching versions of Parked and Socialize on the server and every participating client. Socialize 1.0.9 or later is required.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
