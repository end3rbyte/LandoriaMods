# Structure Protection

Protects offline players' structures from deliberate creature targeting and blocks player weapon damage inside active wards when no authorized player is online.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Prevents creatures from deliberately targeting those pieces while their creator is offline.
- Blocks player weapon damage inside an active ward while its creator and all permitted players are offline.
- Supports dedicated servers.

Pieces without an identifiable player creator retain vanilla behavior.

Creatures use vanilla targeting while the creator is online and do not deliberately target the piece while the creator is offline. Creature attacks still apply vanilla damage if they hit the piece.

An active ward uses vanilla player weapon damage while its creator or any permitted player is online. When all of them are offline, the ward blocks player weapon damage inside its radius. Player interactions, creature damage, environmental damage, and damage outside active wards retain vanilla behavior.

## Dedicated server configuration

Structure Protection reads these command-line switches only on a dedicated server at startup:

| Switch | Default | Behavior |
|---|---:|---|
| `--structure-protection-creature-targeting true\|false` | `true` | Prevents deliberate creature targeting while a piece creator is offline. |
| `--structure-protection-ward-player-damage true\|false` | `true` | Blocks player weapon damage in an unattended active ward. |

The server sends only the creature-targeting setting to clients after they spawn. Settings remain unchanged until the dedicated server restarts.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

Install matching versions of Structure Protection on the server and every participating client.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
