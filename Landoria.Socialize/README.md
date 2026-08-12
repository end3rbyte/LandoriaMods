# Socialize

Adds persistent player groups, private messaging, map sharing, and expanded chat channels.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Creates persistent server-owned groups of up to five players.
- Gives leaders invite, remove, and promotion controls.
- Adds nearby, shout, whisper, private ping, and group chat channels.
- Keeps the selected shout, whisper, or group channel active.
- Automatically shares connected group members' map positions.
- Restricts public map positions and pings outside groups.
- Uses vanilla rain wear and fuel consumption while a piece's creator or one of their group members is connected.
- Stops rain wear and fuel consumption for player-built pieces while the creator and their entire group are offline.
- Prevents creatures from targeting or damaging those pieces while the creator and their entire group are offline.
- Restricts interaction with player-built pieces to their creator and members of the creator's group.
- Shows the creator's name while hovering over a player-built piece that the local player cannot use.
- Supports dedicated servers and peer-hosted worlds.

## Chat Commands

| Command | Purpose | Display |
|---|---|---|
| `/s <message>` or `/say <message>` | Sends normal nearby chat. | Vanilla white |
| `/sh <message>` or `/shout <message>` | Shouts within twice the normal say range. | Orange sender, yellow text |
| `/w <PlayerName> <message>` | Sends a world-wide private message. | Green |
| `/wping <PlayerName> <message>` | Sends a private message and animated map ping. | Green with yellow label |
| `/g <message>` | Sends a message to connected group members. | Blue |

## Group Commands

| Command | Purpose | Access |
|---|---|---|
| `/group help` | Lists group commands. | Everyone |
| `/group invite <PlayerName>` | Invites a connected player. | Leader |
| `/group leave` | Leaves the current group. | Member |
| `/group remove <PlayerName>` | Removes a member. | Leader |
| `/group promote <PlayerName>` | Transfers leadership. | Leader |
| `/group info` | Lists members, connection state, and leader. | Member |

Groups with fewer than two members are disbanded automatically. Invitations use Valheim's Yes/No popup.

## Map Sharing

- The vanilla public-position option is disabled and hidden.
- Connected group members are visible to each other automatically.
- The vanilla map-ping button and public pings require group membership.
- `/wping` remains available without a group.
- Friendly-fire rules, permissions, and teleportation are provided by other mods.

Group data is serialized in a persistent server-owned ZDO stored with the world. No separate configuration or group file is created.

Pieces without an identifiable player creator retain vanilla behavior.
Fuel does not catch up retroactively when a protected fireplace or torch is loaded again.

Piece interaction protection includes normal use, using an item on a piece, containers, doors, crafting stations, adding resources or fuel, repairs, hammer removal, and damage caused by a player. Creatures use vanilla targeting and damage whenever the creator or a group member is online, and ignore the piece while the entire group is offline. Environmental damage and interactions with natural or creatorless objects retain vanilla behavior.

Denied interactions are silent. Hovering an inaccessible piece shows its creator instead. New pieces store the creator name directly; legacy pieces use names already known by Socialize and may display `Unknown creator` until the creator is identified.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

Install matching versions on the server and every participating client.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
