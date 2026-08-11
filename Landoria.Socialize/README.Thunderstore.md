# Socialize

Adds persistent player groups, private messaging, map sharing, and expanded chat channels.

## Features

- Creates persistent groups of up to five players.
- Gives group leaders invite, remove, and promotion controls.
- Adds nearby, shout, private, private-ping, and group chat.
- Keeps the selected chat channel active.
- Shares connected group members' map positions automatically.
- Restricts public positions and map pings outside groups.
- Uses vanilla rain wear and fuel consumption while a piece's creator or one of their group members is connected.
- Stops rain wear and fuel consumption while the creator and their entire group are offline.
- Restricts use, refueling, repairs, removal, and player damage for player-built pieces to their creator and group.

## Chat Commands

| Command | Purpose |
|---|---|
| `/s <message>` or `/say <message>` | Sends nearby chat. |
| `/sh <message>` or `/shout <message>` | Shouts within twice the normal say range. |
| `/w <PlayerName> <message>` | Sends a world-wide private message. |
| `/wping <PlayerName> <message>` | Sends a private message and animated ping. |
| `/g <message>` | Messages connected group members. |

## Group Commands

| Command | Purpose |
|---|---|
| `/group help` | Lists group commands. |
| `/group invite <PlayerName>` | Invites a connected player. |
| `/group leave` | Leaves the group. |
| `/group remove <PlayerName>` | Removes a member; leader only. |
| `/group promote <PlayerName>` | Transfers leadership. |
| `/group info` | Lists group members and status. |

- Groups persist in the world and disband below two members.
- Invitations use Valheim's Yes/No popup.
- Friendly-fire rules, permissions, and teleportation are not included.

## Installation

| Client required | Server required |
|---|---|
| Yes | Yes |

See the [full documentation](https://github.com/end3rbyte/LandoriaMods/blob/main/Landoria.Socialize/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/end3rbyte/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/end3rbyte/LandoriaMods/discussions).
