# Socialize

Adds temporary player groups for missions and expeditions, private messaging, map sharing, and expanded chat channels.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Lets up to five connected players form a temporary group for a mission or expedition.
- Gives leaders invite, remove, and promotion controls.
- Adds nearby, shout, server-wide, whisper, private ping, and group chat channels.
- Keeps the selected shout, whisper, or group channel active.
- Automatically shares connected group members' map positions.
- Restricts public map positions and pings outside groups.
- Supports dedicated servers.

## Chat Commands

| Command | Purpose | Display |
|---|---|---|
| `/s <message>` or `/say <message>` | Sends normal nearby chat. | Vanilla white |
| `/sh <message>` or `/shout <message>` | Shouts within the configured local range. | Orange sender, yellow text |
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

Groups last only for the current connection. A player leaves their group automatically when they disconnect and is not placed back in that group when they reconnect. If the leader disconnects, the longest-standing remaining member becomes the new leader. Groups with fewer than two members are disbanded automatically. Invitations use Valheim's Yes/No popup.

## Map Sharing

- The vanilla public-position option is disabled and hidden.
- Connected group members are visible to each other automatically.
- The vanilla map-ping button and public pings require group membership.
- `/wping` remains available without a group.
- Friendly-fire rules, permissions, and teleportation are provided by other mods.

Group data is held only in server memory and is not stored with the world. Player membership ends on disconnect, and all remaining group data is cleared when the server stops.

## Configuration

| Dedicated-server switch | Default | Behavior |
|---|---:|---|
| `--socialize-restrict-public-positions true\|false` | `true` | Hides and disables vanilla public-position sharing. |
| `--socialize-restrict-public-pings true\|false` | `true` | Restricts the public map-ping button and delivery to group members. |

Distance values must be positive finite numbers. The server reads these switches once,
keeps the effective configuration in memory, and sends it to each client after spawning.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

Install matching versions on the server and every participating client.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
