# ExpandedServer

Raises Valheim's server capacity to a configurable player limit.

## Valheim compatibility

| Valheim channel | Version | Compatibility |
|---|---:|---|
| Current release | `0.221.12` | Compatible |
| Public Test | `0.221.13` | Compatible |

## Features

- Increases server capacity to up to 100 players, with a default limit of 20.
- Updates admission checks and PlayFab advertised capacity.
- Applies its expanded capacity on dedicated servers.
- Limits configured values to a maximum of 100 players.

## Valheim.exe Command Switches

| Switch | Default | Purpose |
|---|---:|---|
| `--maxplayer <1-100>` | `20` | Sets dedicated-server capacity. Invalid values use 20; values above 100 use 100. |

Joining clients need the mod but do not use the switch. PlayFab Party may impose a lower backend capacity than the configured value.

## Installation

| Client required | Server required (dedicated) | Player-hosted server |
|---|---|---|
| Yes | Yes | Not Supported |

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/LandoriaMods/issues).
For questions, feedback, and other discussions, use [GitHub Discussions](https://github.com/landoria-gaming/LandoriaMods/discussions).
