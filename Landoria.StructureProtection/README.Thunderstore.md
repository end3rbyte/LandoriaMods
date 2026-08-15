# Structure Protection

Prevents creatures from deliberately targeting player-built pieces while their creator is offline.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Prevents creatures from deliberately targeting pieces while their creator is offline, without blocking incidental creature damage.
- Blocks player weapon damage inside active wards while their creator and all permitted players are offline.
- Keeps vanilla player access, interaction, and creator-name display behavior.

Dedicated servers can disable either feature with `--structure-protection-creature-targeting false` or `--structure-protection-ward-player-damage false`. Both default to `true`.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

Install matching versions of Structure Protection on the server and every participating client.

Read the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.StructureProtection/README.md) on GitHub.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
