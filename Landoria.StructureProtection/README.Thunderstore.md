# Structure Protection

Protects offline players' structures from deliberate creature targeting and blocks player weapon damage inside active wards when no authorized player is online.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Prevents creatures from deliberately targeting player-built structures while their creator is offline, without blocking damage if a creature happens to hit them.
- Protects structures inside an active ward from player weapon damage while its creator and all permitted players are offline.
- Keeps vanilla player access, interaction, and creator-name display behavior.

Both protections are enabled by default. Dedicated-server administrators can configure them separately with `--structure-protection-creature-targeting` and `--structure-protection-ward-player-damage`.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

Install matching versions of Structure Protection on the server and every participating client.

Read the [full documentation](https://github.com/landoria-gaming/LandoriaMods/blob/main/Landoria.StructureProtection/README.md) on GitHub.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
