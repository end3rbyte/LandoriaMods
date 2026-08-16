# Socialize

Adds temporary player groups for missions and expeditions, private messaging, map sharing, and expanded chat channels.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Lets up to five connected players form a temporary group for a mission or expedition.
- Gives group leaders invite, remove, and promotion controls.
- Adds nearby, shout, server-wide, group, and private chat, with optional private map pings.
- Keeps the selected chat channel active.
- Shares connected group members' map positions automatically.
- Restricts public positions and map pings outside groups.

## Chat Commands

| Command | Purpose |
|---|---|
| `/s <message>` or `/say <message>` | Sends nearby chat. |
| `/sh <message>` or `/shout <message>` | Shouts within the configured local range. |
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

- Group membership lasts only for the current connection: players leave when they disconnect and are not placed back in the group when they reconnect.
- When the leader disconnects, the longest-standing remaining member becomes the new leader.
- A group ends when fewer than two members remain connected.
- Invitations use Valheim's Yes/No popup.
- Friendly-fire rules, permissions, and teleportation are not included.

## Configuration

| Dedicated-server switch | Default |
|---|---:|
| `--socialize-restrict-public-positions true\|false` | `true` |
| `--socialize-restrict-public-pings true\|false` | `true` |
| `--socialize-shout-distance <metres>` | `30` |
| `--socialize-say-distance <metres>` | `15` |

Distance values must be positive finite numbers. The server reads these switches once
and sends its in-memory configuration to each client after spawning.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

See the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.Socialize/README.md).

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
